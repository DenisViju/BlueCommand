using System.Globalization;
using BlueCommand.API.DTOs.Common;
using BlueCommand.API.DTOs.Dosare;
using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Constants;
using BlueCommand.Domain.Entities;
using BlueCommand.Domain.Enums;
using BlueCommand.Infrastructure.Data;
using BlueCommand.Infrastructure.Utils;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/dosare")]
[Authorize(Policy = "TotiUtilizatorii")]
public class DosareController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"
    };

    private readonly BlueCommandDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public DosareController(BlueCommandDbContext db, IAuditService audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<DosarListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? sectieId,
        [FromQuery] string? tipIncident,
        [FromQuery] DateOnly? dataStart,
        [FromQuery] DateOnly? dataEnd,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;
        page = Math.Max(page, 1);

        IQueryable<Dosar> query = _db.Dosare.AsNoTracking()
            .Include(d => d.Sectie)
            .Include(d => d.DosarAgenti);

        query = ApplyRoleFilter(query);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.NumarDosar.Contains(search) || (d.Titlu != null && d.Titlu.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DosarStatus>(status, true, out var st))
            query = query.Where(d => d.Status == st);

        if (sectieId.HasValue)
            query = query.Where(d => d.SectieId == sectieId.Value);

        if (!string.IsNullOrWhiteSpace(tipIncident))
            query = query.Where(d => d.TipIncident != null && d.TipIncident.Contains(tipIncident));

        if (dataStart.HasValue)
            query = query.Where(d => d.DataIncident != null && d.DataIncident.Value >= dataStart.Value);

        if (dataEnd.HasValue)
            query = query.Where(d => d.DataIncident != null && d.DataIncident.Value <= dataEnd.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(d => d.CreatLa)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DosarListDto
            {
                Id = d.Id,
                NumarDosar = d.NumarDosar,
                Titlu = d.Titlu,
                TipIncident = d.TipIncident,
                DataIncident = d.DataIncident,
                Status = d.Status.ToString(),
                SectieId = d.SectieId,
                SectieNume = d.Sectie.Nume,
                CreatLa = d.CreatLa
            })
            .ToListAsync(cancellationToken);

        return Ok(new PagedResultDto<DosarListDto> { Items = items, Total = total, Page = page, PageSize = pageSize });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DosarDetailDto>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = _db.Dosare.AsNoTracking()
            .Include(d => d.Sectie)
            .Include(d => d.DosarAgenti).ThenInclude(da => da.Utilizator)
            .Include(d => d.Documente)
            .Include(d => d.Istoric);

        var dosar = await ApplyRoleFilter(query).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dosar is null) return NotFound();

        var dto = new DosarDetailDto
        {
            Id = dosar.Id,
            NumarDosar = dosar.NumarDosar,
            Titlu = dosar.Titlu,
            Descriere = dosar.Descriere,
            Status = dosar.Status.ToString(),
            TipIncident = dosar.TipIncident,
            DataIncident = dosar.DataIncident,
            SectieId = dosar.SectieId,
            SectieNume = dosar.Sectie.Nume,
            CreatDe = dosar.CreatDe,
            CreatLa = dosar.CreatLa,
            ActualizatLa = dosar.ActualizatLa,
            Agenti = dosar.DosarAgenti
                .Select(da => da.Utilizator)
                .OrderBy(u => u.Id)
                .Select(u => new DosarAgentDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Nume = u.Nume,
                    Prenume = u.Prenume,
                    Grad = u.Grad
                })
                .ToList(),
            Documente = dosar.Documente
                .OrderByDescending(x => x.DataIncarcare)
                .Select(x => new DosarDocumentDto
                {
                    Id = x.Id,
                    NumeFisier = x.NumeFisier,
                    CaleFisier = x.CaleFisier,
                    MarimeBytes = x.MarimeBytes,
                    IncarcatDe = x.IncarcatDe,
                    DataIncarcare = x.DataIncarcare
                })
                .ToList(),
            Istoric = dosar.Istoric
                .OrderByDescending(i => i.ModificatLa)
                .Select(i => new IstoricItemDto
                {
                    Id = i.Id,
                    CampModificat = i.CampModificat,
                    ValoareVeche = i.ValoareVeche,
                    ValoareNoua = i.ValoareNoua,
                    ModificatDe = i.ModificatDe,
                    ModificatLa = i.ModificatLa
                })
                .ToList()
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDosarRequestDto request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == RoluriDenumiri.Administrator)
            return Forbid();

        var exists = await _db.Dosare.AnyAsync(d => d.NumarDosar == request.NumarDosar, cancellationToken);
        if (exists)
            return BadRequest(new { error = "Numarul de dosar exista deja in sistem" });

        var sectie = await _db.Sectii.FirstOrDefaultAsync(s => s.Id == request.SectieId, cancellationToken);
        if (sectie is null)
            return BadRequest(new { error = "Sectie invalida" });

        var agentRoleId = await _db.Roluri.Where(r => r.Denumire == RoluriDenumiri.AgentPolitie).Select(r => r.Id).FirstAsync(cancellationToken);
        var agents = await _db.Utilizatori
            .Where(u => request.AgentiIds.Contains(u.Id) && u.RolId == agentRoleId && u.EsteActiv)
            .ToListAsync(cancellationToken);

        if (agents.Count != request.AgentiIds.Count)
            return BadRequest(new { error = "Lista agentilor este invalida" });

        if (agents.Any(a => a.SectieId != request.SectieId))
            return BadRequest(new { error = "Toti agentii trebuie sa apartina sectiei selectate" });

        var creatorId = _currentUser.UserId ?? 0;
        var dosar = new Dosar
        {
            NumarDosar = request.NumarDosar,
            Titlu = request.Titlu,
            Descriere = request.Descriere,
            TipIncident = request.TipIncident,
            DataIncident = request.DataIncident,
            SectieId = request.SectieId,
            Status = DosarStatus.DESCHIS,
            CreatDe = creatorId,
            CreatLa = DateTime.UtcNow
        };

        _db.Dosare.Add(dosar);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var agent in agents)
            _db.DosarAgenti.Add(new DosarAgent { DosarId = dosar.Id, UtilizatorId = agent.Id });

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "CREATE_DOSAR", $"Created dosar {dosar.NumarDosar}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dosar.Id }, new { id = dosar.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateDosarRequestDto request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == RoluriDenumiri.Administrator)
            return Forbid();

        var dosar = await _db.Dosare
            .Include(d => d.DosarAgenti)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dosar is null) return NotFound();

        if (!await CanAccessDosarAsync(dosar, cancellationToken))
            return Forbid();

        if (dosar.Status == DosarStatus.INCHIS)
            return BadRequest(new { error = "Dosarul este inchis si nu poate fi modificat." });

        var original = new Snapshot(dosar);

        if (request.Titlu is not null) dosar.Titlu = request.Titlu;
        if (request.Descriere is not null) dosar.Descriere = request.Descriere;
        if (request.TipIncident is not null) dosar.TipIncident = request.TipIncident;
        if (request.DataIncident is not null) dosar.DataIncident = request.DataIncident;
        if (request.SectieId is not null) dosar.SectieId = request.SectieId.Value;
        if (request.Status is not null && Enum.TryParse<DosarStatus>(request.Status, true, out var newStatus))
            dosar.Status = newStatus;

        dosar.ActualizatLa = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var updated = new Snapshot(dosar);
        var modifiedBy = _currentUser.UserId ?? 0;
        foreach (var (field, oldVal, newVal) in ChangeTracker.Diff(original, updated))
            _db.IstoricDosare.Add(ChangeTracker.ToIstoricDosar(dosar.Id, field, oldVal, newVal, modifiedBy));

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogActionAsync(_currentUser.UserId, "UPDATE_DOSAR", $"Updated dosar {dosar.NumarDosar}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpPut("{id:int}/inchide")]
    [Authorize(Policy = "SefSauAdmin")]
    public async Task<IActionResult> Inchide([FromRoute] int id, CancellationToken cancellationToken)
    {
        var dosar = await _db.Dosare.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dosar is null) return NotFound();

        dosar.Status = DosarStatus.INCHIS;
        dosar.ActualizatLa = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var modifiedBy = _currentUser.UserId ?? 0;
        _db.IstoricDosare.Add(ChangeTracker.ToIstoricDosar(dosar.Id, "Status", null, DosarStatus.INCHIS.ToString(), modifiedBy));
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "INCHIDE_DOSAR", $"Closed dosar {dosar.NumarDosar}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpPut("{id:int}/redeschide")]
    [Authorize(Policy = "SefSauAdmin")]
    public async Task<IActionResult> Redeschide([FromRoute] int id, CancellationToken cancellationToken)
    {
        var dosar = await _db.Dosare.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dosar is null) return NotFound();

        if (dosar.Status != DosarStatus.INCHIS)
            return BadRequest(new { error = "Dosarul nu este inchis." });

        var oldStatus = dosar.Status.ToString();
        dosar.Status = DosarStatus.DESCHIS;
        dosar.ActualizatLa = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var modifiedBy = _currentUser.UserId ?? 0;
        _db.IstoricDosare.Add(ChangeTracker.ToIstoricDosar(dosar.Id, "Status", oldStatus, DosarStatus.DESCHIS.ToString(), modifiedBy));
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "REDESCHIDE_DOSAR", $"Reopened dosar {dosar.NumarDosar}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpPut("{id:int}/agenti")]
    [Authorize(Policy = "SefSauAdmin")]
    public async Task<IActionResult> UpdateAgenti([FromRoute] int id, [FromBody] UpdateDosarAgentiRequestDto request, CancellationToken cancellationToken)
    {
        var dosar = await _db.Dosare
            .Include(d => d.DosarAgenti)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dosar is null) return NotFound();
        if (dosar.Status == DosarStatus.INCHIS)
            return BadRequest(new { error = "Dosarul este inchis si nu poate fi modificat." });

        var agentRoleId = await _db.Roluri.Where(r => r.Denumire == RoluriDenumiri.AgentPolitie).Select(r => r.Id).FirstAsync(cancellationToken);
        var agents = await _db.Utilizatori
            .Where(u => request.AgentiIds.Contains(u.Id) && u.RolId == agentRoleId && u.EsteActiv)
            .ToListAsync(cancellationToken);

        if (agents.Count != request.AgentiIds.Count)
            return BadRequest(new { error = "Lista agentilor este invalida" });

        dosar.DosarAgenti.RemoveAll(da => true);
        foreach (var a in agents)
            dosar.DosarAgenti.Add(new DosarAgent { DosarId = dosar.Id, UtilizatorId = a.Id });

        dosar.ActualizatLa = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var modifiedBy = _currentUser.UserId ?? 0;
        _db.IstoricDosare.Add(ChangeTracker.ToIstoricDosar(dosar.Id, "Agenti", null, string.Join(",", request.AgentiIds), modifiedBy));
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "UPDATE_DOSAR_AGENTI", $"Updated agents for dosar {dosar.NumarDosar}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpGet("{id:int}/istoric")]
    public async Task<IActionResult> Istoric([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = _db.IstoricDosare.AsNoTracking().Where(i => i.DosarId == id);
        var items = await query.OrderByDescending(i => i.ModificatLa)
            .Select(i => new IstoricItemDto
            {
                Id = i.Id,
                CampModificat = i.CampModificat,
                ValoareVeche = i.ValoareVeche,
                ValoareNoua = i.ValoareNoua,
                ModificatDe = i.ModificatDe,
                ModificatLa = i.ModificatLa
            })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("{id:int}/documente")]
    public async Task<IActionResult> UploadDocument([FromRoute] int id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == RoluriDenumiri.Administrator)
            return Forbid();

        var dosar = await _db.Dosare.Include(d => d.DosarAgenti).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dosar is null) return NotFound();

        if (!await CanAccessDosarAsync(dosar, cancellationToken))
            return Forbid();

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "Fisierul depaseste limita de 10 MB" });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            return BadRequest(new { error = "Extensie de fisier nepermisa" });

        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", id.ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(uploadsRoot);

        var storedName = $"{Guid.NewGuid():N}{ext}";
        var storedPath = Path.Combine(uploadsRoot, storedName);

        await using (var stream = System.IO.File.Create(storedPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"/uploads/{id}/{storedName}";

        var doc = new DocumentDosar
        {
            DosarId = id,
            NumeFisier = file.FileName,
            CaleFisier = relativePath,
            MarimeBytes = file.Length,
            IncarcatDe = _currentUser.UserId ?? 0,
            DataIncarcare = DateTime.UtcNow
        };

        _db.DocumenteDosar.Add(doc);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "UPLOAD_DOCUMENT", $"Uploaded document {file.FileName} for dosar {dosar.NumarDosar}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(new { id = doc.Id, doc.CaleFisier });
    }

    [HttpDelete("{id:int}/documente/{documentId:int}")]
    public async Task<IActionResult> DeleteDocument([FromRoute] int id, [FromRoute] int documentId, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == RoluriDenumiri.Administrator)
            return Forbid();

        var dosar = await _db.Dosare.Include(d => d.DosarAgenti).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dosar is null) return NotFound();

        if (!await CanAccessDosarAsync(dosar, cancellationToken))
            return Forbid();

        var doc = await _db.DocumenteDosar.FirstOrDefaultAsync(d => d.Id == documentId && d.DosarId == id, cancellationToken);
        if (doc is null) return NotFound();

        _db.DocumenteDosar.Remove(doc);
        await _db.SaveChangesAsync(cancellationToken);

        var physical = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.CaleFisier.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(physical))
            System.IO.File.Delete(physical);

        await _audit.LogActionAsync(_currentUser.UserId, "DELETE_DOCUMENT", $"Deleted document {doc.NumeFisier} for dosar {dosar.NumarDosar}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpGet("{id:int}/export")]
    public async Task<IActionResult> Export([FromRoute] int id, [FromQuery] string format, CancellationToken cancellationToken)
    {
        var dosar = await _db.Dosare.AsNoTracking()
            .Include(d => d.Sectie)
            .Include(d => d.DosarAgenti).ThenInclude(da => da.Utilizator)
            .Include(d => d.Documente)
            .Include(d => d.Istoric)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dosar is null) return NotFound();

        if (!await CanAccessDosarAsync(dosar, cancellationToken))
            return Forbid();

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdf = GenerateDosarPdf(dosar);
            return File(pdf, "application/pdf", $"dosar_{dosar.NumarDosar.Replace('/', '_')}.pdf");
        }

        if (string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase))
        {
            var content = GenerateDosarExcel(dosar);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"dosar_{dosar.NumarDosar.Replace('/', '_')}.xlsx");
        }

        return BadRequest(new { error = "Format invalid" });
    }

    private IQueryable<Dosar> ApplyRoleFilter(IQueryable<Dosar> query)
    {
        var role = _currentUser.Role;
        if (role == RoluriDenumiri.Administrator || role == RoluriDenumiri.SefInspectorat)
            return query;

        if (role == RoluriDenumiri.AgentPolitie)
        {
            var userId = _currentUser.UserId ?? -1;
            var sectieId = _currentUser.SectieId ?? -1;
            return query.Where(d =>
                d.SectieId == sectieId ||
                d.DosarAgenti.Any(da => da.UtilizatorId == userId));
        }

        return query.Where(_ => false);
    }

    private async Task<bool> CanAccessDosarAsync(Dosar dosar, CancellationToken cancellationToken)
    {
        var role = _currentUser.Role;
        if (role == RoluriDenumiri.Administrator || role == RoluriDenumiri.SefInspectorat)
            return true;

        if (role != RoluriDenumiri.AgentPolitie)
            return false;

        var userId = _currentUser.UserId ?? -1;
        var sectieId = _currentUser.SectieId;
        if (sectieId.HasValue && dosar.SectieId == sectieId.Value)
            return true;

        return await _db.DosarAgenti.AnyAsync(da => da.DosarId == dosar.Id && da.UtilizatorId == userId, cancellationToken);
    }

    private static byte[] GenerateDosarPdf(Dosar dosar)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Header()
                    .Text($"Dosar {dosar.NumarDosar}")
                    .FontSize(18)
                    .SemiBold();

                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Titlu: {dosar.Titlu}");
                    col.Item().Text($"Status: {dosar.Status}");
                    col.Item().Text($"Tip incident: {dosar.TipIncident}");
                    col.Item().Text($"Sectie: {dosar.SectieId} - {dosar.Sectie.Nume}");
                    col.Item().Text($"Data incident: {dosar.DataIncident}");
                    col.Item().Text("Descriere:");
                    col.Item().Text(dosar.Descriere ?? "-").FontColor(Colors.Grey.Darken2);

                    col.Item().Text("Agenti:");
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Username");
                            h.Cell().Element(CellStyle).Text("Nume");
                            h.Cell().Element(CellStyle).Text("Grad");
                        });
                        foreach (var a in dosar.DosarAgenti.Select(x => x.Utilizator).OrderBy(x => x.Id))
                        {
                            t.Cell().Element(CellStyle).Text(a.Username);
                            t.Cell().Element(CellStyle).Text($"{a.Nume} {a.Prenume}".Trim());
                            t.Cell().Element(CellStyle).Text(a.Grad ?? "-");
                        }
                    });

                    col.Item().Text("Documente:");
                    col.Item().Text(string.Join(", ", dosar.Documente.Select(d => d.NumeFisier))).FontColor(Colors.Grey.Darken2);

                    col.Item().Text("Istoric:");
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Camp");
                            h.Cell().Element(CellStyle).Text("Vechi");
                            h.Cell().Element(CellStyle).Text("Nou");
                        });
                        foreach (var i in dosar.Istoric.OrderByDescending(x => x.ModificatLa).Take(50))
                        {
                            t.Cell().Element(CellStyle).Text(i.CampModificat);
                            t.Cell().Element(CellStyle).Text(i.ValoareVeche ?? "-");
                            t.Cell().Element(CellStyle).Text(i.ValoareNoua ?? "-");
                        }
                    });
                });
            });
        });

        return doc.GeneratePdf();

        static IContainer CellStyle(IContainer c) =>
            c.PaddingVertical(4).PaddingHorizontal(6).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
    }

    private static byte[] GenerateDosarExcel(Dosar dosar)
    {
        using var workbook = new XLWorkbook();
        var details = workbook.Worksheets.Add("Detalii Dosar");
        details.Cell(1, 1).Value = "Numar Dosar";
        details.Cell(1, 2).Value = dosar.NumarDosar;
        details.Cell(2, 1).Value = "Titlu";
        details.Cell(2, 2).Value = dosar.Titlu ?? "-";
        details.Cell(3, 1).Value = "Status";
        details.Cell(3, 2).Value = dosar.Status.ToString();
        details.Cell(4, 1).Value = "Tip Incident";
        details.Cell(4, 2).Value = dosar.TipIncident ?? "-";
        details.Cell(5, 1).Value = "Sectie";
        details.Cell(5, 2).Value = dosar.Sectie.Nume;
        details.Cell(6, 1).Value = "Data Incident";
        details.Cell(6, 2).Value = dosar.DataIncident?.ToString() ?? "-";
        details.Columns().AdjustToContents();

        var history = workbook.Worksheets.Add("Istoric");
        history.Cell(1, 1).Value = "Camp";
        history.Cell(1, 2).Value = "Vechi";
        history.Cell(1, 3).Value = "Nou";
        history.Cell(1, 4).Value = "ModificatLa";
        var row = 2;
        foreach (var i in dosar.Istoric.OrderByDescending(x => x.ModificatLa))
        {
            history.Cell(row, 1).Value = i.CampModificat;
            history.Cell(row, 2).Value = i.ValoareVeche ?? "-";
            history.Cell(row, 3).Value = i.ValoareNoua ?? "-";
            history.Cell(row, 4).Value = i.ModificatLa;
            row++;
        }
        history.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private sealed record Snapshot(Dosar d)
    {
        public string? Titlu { get; init; } = d.Titlu;
        public string? Descriere { get; init; } = d.Descriere;
        public string? TipIncident { get; init; } = d.TipIncident;
        public DateOnly? DataIncident { get; init; } = d.DataIncident;
        public int SectieId { get; init; } = d.SectieId;
        public DosarStatus Status { get; init; } = d.Status;
    }
}