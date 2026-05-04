using BlueCommand.API.DTOs.Rapoarte;
using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Constants;
using BlueCommand.Domain.Enums;
using BlueCommand.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/rapoarte")]
[Authorize(Policy = "SefSauAdmin")]
public class RapoarteController : ControllerBase
{
    private readonly BlueCommandDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RapoarteController(BlueCommandDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? 0;
        var items = await _db.Rapoarte.AsNoTracking()
            .Where(r => r.UtilizatorId == userId)
            .OrderByDescending(r => r.DataGenerare)
            .Select(r => new { r.Id, r.Tip, r.FiltruPerioada, r.DataGenerare, r.CaleFisier })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("genereaza")]
    public async Task<IActionResult> Genereaza([FromBody] GenerateRaportRequestDto request, CancellationToken cancellationToken)
    {
        if (request.DataStart > request.DataEnd)
            return BadRequest(new { error = "Data de început trebuie să fie înainte de data de sfârșit." });
        if (!IsValidTip(request.Tip))
            return BadRequest(new { error = "Tip raport invalid" });

        var data = await GenerateReportData(request, cancellationToken);

        var filtru = $"{request.DataStart:yyyy-MM-dd}|{request.DataEnd:yyyy-MM-dd}";
        var raport = new BlueCommand.Domain.Entities.Raport
        {
            UtilizatorId = _currentUser.UserId ?? 0,
            Tip = request.Tip,
            FiltruPerioada = filtru,
            DataGenerare = DateTime.UtcNow,
            CaleFisier = null
        };
        _db.Rapoarte.Add(raport);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { raportId = raport.Id, data });
    }

    [HttpPost("export")]
    public async Task<IActionResult> Export([FromBody] ExportRaportRequestDto request, CancellationToken cancellationToken)
    {
        if (request.DataStart > request.DataEnd)
            return BadRequest(new { error = "Data de început trebuie să fie înainte de data de sfârșit." });
        if (!IsValidTip(request.Tip))
            return BadRequest(new { error = "Tip raport invalid" });

        var data = await GenerateReportData(request, cancellationToken);

        if (string.Equals(request.Format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdf = GenerateReportPdf(request, data);
            return File(pdf, "application/pdf", $"raport_{request.Tip}_{DateTime.UtcNow:yyyyMMddHHmm}.pdf");
        }

        if (string.Equals(request.Format, "excel", StringComparison.OrdinalIgnoreCase))
        {
            var xlsx = GenerateReportExcel(request, data);
            return File(xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"raport_{request.Tip}_{DateTime.UtcNow:yyyyMMddHHmm}.xlsx");
        }

        return BadRequest(new { error = "Format invalid" });
    }

    private async Task<object> GenerateReportData(GenerateRaportRequestDto request, CancellationToken cancellationToken)
    {
        var start = request.DataStart;
        var end = request.DataEnd;

        var dosare = _db.Dosare.AsNoTracking()
            .Where(d => d.DataIncident != null && d.DataIncident.Value >= start && d.DataIncident.Value <= end);

        if (request.SectieId.HasValue)
            dosare = dosare.Where(d => d.SectieId == request.SectieId.Value);

        switch (request.Tip)
        {
            case "INCIDENTE":
                return await dosare
                    .GroupBy(d => new { Year = d.DataIncident!.Value.Year, Month = d.DataIncident!.Value.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new { luna = $"{g.Key.Year}-{g.Key.Month:00}", count = g.Count() })
                    .ToListAsync(cancellationToken);

            case "STATISTICI_SECTII":
                return await dosare
                    .GroupBy(d => d.SectieId)
                    .Select(g => new
                    {
                        sectieId = g.Key,
                        total = g.Count(),
                        deschise = g.Count(x => x.Status == DosarStatus.DESCHIS),
                        inLucru = g.Count(x => x.Status == DosarStatus.IN_LUCRU),
                        inchise = g.Count(x => x.Status == DosarStatus.INCHIS)
                    })
                    .OrderBy(x => x.sectieId)
                    .ToListAsync(cancellationToken);

            case "TIP_INCIDENT":
                return await dosare
                    .GroupBy(d => d.TipIncident ?? "Necunoscut")
                    .Select(g => new { tip = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .ToListAsync(cancellationToken);

            case "ACTIVITATE_AGENTI":
                var q = _db.DosarAgenti.AsNoTracking()
                    .Include(da => da.Utilizator)
                    .Include(da => da.Dosar)
                    .Where(da => da.Dosar.DataIncident != null && da.Dosar.DataIncident.Value >= start && da.Dosar.DataIncident.Value <= end);

                if (request.SectieId.HasValue)
                    q = q.Where(da => da.Dosar.SectieId == request.SectieId.Value);

                var result = await q
                    .GroupBy(da => new { da.UtilizatorId, da.Utilizator.Username, da.Utilizator.Nume, da.Utilizator.Prenume })
                    .Select(g => new
                    {
                        agentId = g.Key.UtilizatorId,
                        nume = $"{g.Key.Nume} {g.Key.Prenume}".Trim(),
                        username = g.Key.Username,
                        total = g.Count(),
                        deschise = g.Count(x => x.Dosar.Status == DosarStatus.DESCHIS || x.Dosar.Status == DosarStatus.IN_LUCRU),
                        inchise = g.Count(x => x.Dosar.Status == DosarStatus.INCHIS)
                    })
                    .OrderByDescending(x => x.total)
                    .ToListAsync(cancellationToken);

                return result;

            default:
                return Array.Empty<object>();
        }
    }

    private static bool IsValidTip(string tip) =>
        tip is "INCIDENTE" or "ACTIVITATE_AGENTI" or "STATISTICI_SECTII" or "TIP_INCIDENT";

    private static byte[] GenerateReportPdf(GenerateRaportRequestDto request, object data)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Header().Text($"Raport: {request.Tip} ({request.DataStart:yyyy-MM-dd} - {request.DataEnd:yyyy-MM-dd})").FontSize(16).SemiBold();
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text("Rezultat (format tabel):").SemiBold();
                    col.Item().Text(System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }))
                        .FontColor(Colors.Grey.Darken2)
                        .FontSize(10);
                });
            });
        });
        return doc.GeneratePdf();
    }

    private static byte[] GenerateReportExcel(GenerateRaportRequestDto request, object data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(request.Tip);
        ws.Cell(1, 1).Value = "Data";
        ws.Cell(1, 2).Value = System.Text.Json.JsonSerializer.Serialize(data);
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
