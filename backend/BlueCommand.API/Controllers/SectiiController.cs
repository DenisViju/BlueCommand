using BlueCommand.API.DTOs.Sectii;
using BlueCommand.Application.Interfaces;
using BlueCommand.Infrastructure.Data;
using BlueCommand.Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/sectii")]
public class SectiiController : ControllerBase
{
    private readonly BlueCommandDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public SectiiController(BlueCommandDbContext db, ICurrentUserService currentUser, IAuditService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    [HttpGet]
    [Authorize(Policy = "TotiUtilizatorii")]
    public async Task<ActionResult<List<SectieDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var query = _db.Sectii.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Nume.Contains(search) || (s.Zona != null && s.Zona.Contains(search)));

        var items = await query.OrderBy(s => s.Id)
            .Select(s => new SectieDto
            {
                Id = s.Id,
                Nume = s.Nume,
                Adresa = s.Adresa,
                Zona = s.Zona,
                Latitudine = s.Latitudine,
                Longitudine = s.Longitudine,
                CreatLa = s.CreatLa
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "TotiUtilizatorii")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var sectie = await _db.Sectii.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sectie is null) return NotFound();

        var agenti = await _db.Utilizatori.AsNoTracking()
            .Include(u => u.Rol)
            .Where(u => u.SectieId == id && u.Rol.Denumire == "AgentPolitie" && u.EsteActiv)
            .OrderBy(u => u.Id)
            .Select(u => new { u.Id, u.Username, u.Nume, u.Prenume, u.Grad, u.EsteActiv })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            sectie = new SectieDto
            {
                Id = sectie.Id,
                Nume = sectie.Nume,
                Adresa = sectie.Adresa,
                Zona = sectie.Zona,
                Latitudine = sectie.Latitudine,
                Longitudine = sectie.Longitudine,
                CreatLa = sectie.CreatLa
            },
            agenti
        });
    }

    [HttpPost]
    [Authorize(Policy = "SefSauAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateSectieRequestDto request, CancellationToken cancellationToken)
    {
        var sectie = new BlueCommand.Domain.Entities.Sectie
        {
            Nume = request.Nume,
            Adresa = request.Adresa,
            Zona = request.Zona,
            Latitudine = request.Latitudine,
            Longitudine = request.Longitudine,
            CreatLa = DateTime.UtcNow
        };

        _db.Sectii.Add(sectie);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogActionAsync(_currentUser.UserId, "CREATE_SECTIE", $"Created sectie {sectie.Nume}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = sectie.Id }, new { id = sectie.Id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "SefSauAdmin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSectieRequestDto request, CancellationToken cancellationToken)
    {
        var sectie = await _db.Sectii.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sectie is null) return NotFound();

        var original = new Snapshot(sectie);

        if (request.Nume is not null) sectie.Nume = request.Nume;
        if (request.Adresa is not null) sectie.Adresa = request.Adresa;
        if (request.Zona is not null) sectie.Zona = request.Zona;
        if (request.Latitudine is not null) sectie.Latitudine = request.Latitudine;
        if (request.Longitudine is not null) sectie.Longitudine = request.Longitudine;

        await _db.SaveChangesAsync(cancellationToken);

        var updated = new Snapshot(sectie);
        var modifiedBy = _currentUser.UserId ?? 0;
        foreach (var (field, oldVal, newVal) in ChangeTracker.Diff(original, updated))
        {
            _db.IstoricSectii.Add(ChangeTracker.ToIstoricSectie(sectie.Id, field, oldVal, newVal, modifiedBy));
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogActionAsync(_currentUser.UserId, "UPDATE_SECTIE", $"Updated sectie {sectie.Nume}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SefSauAdmin")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var sectie = await _db.Sectii.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sectie is null) return NotFound();

        var hasActiveAgents = await _db.Utilizatori.AnyAsync(u => u.SectieId == id && u.EsteActiv, cancellationToken);
        if (hasActiveAgents)
            return BadRequest(new { error = "Sectia are agenti asociati. Reasignati-i inainte de stergere." });

        var hasActiveCases = await _db.Dosare.AnyAsync(d => d.SectieId == id && d.Status != BlueCommand.Domain.Enums.DosarStatus.INCHIS, cancellationToken);
        if (hasActiveCases)
            return BadRequest(new { error = "Sectia are dosare active. Inchideti dosarele inainte de stergere." });

        _db.Sectii.Remove(sectie);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogActionAsync(_currentUser.UserId, "DELETE_SECTIE", $"Deleted sectie {sectie.Nume}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpGet("{id:int}/istoric")]
    [Authorize(Policy = "SefSauAdmin")]
    public async Task<IActionResult> Istoric([FromRoute] int id, CancellationToken cancellationToken)
    {
        var items = await _db.IstoricSectii.AsNoTracking()
            .Where(i => i.SectieId == id)
            .OrderByDescending(i => i.ModificatLa)
            .Select(i => new { i.Id, i.CampModificat, i.ValoareVeche, i.ValoareNoua, i.ModificatDe, i.ModificatLa })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("harta")]
    [Authorize(Policy = "TotiUtilizatorii")]
    public async Task<IActionResult> Harta(CancellationToken cancellationToken)
    {
        var items = await _db.Sectii.AsNoTracking()
            .Where(s => s.Latitudine != null && s.Longitudine != null)
            .OrderBy(s => s.Id)
            .Select(s => new { s.Id, s.Nume, s.Latitudine, s.Longitudine, s.Zona, s.Adresa })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    private sealed record Snapshot(BlueCommand.Domain.Entities.Sectie s)
    {
        public string Nume { get; init; } = s.Nume;
        public string? Adresa { get; init; } = s.Adresa;
        public string? Zona { get; init; } = s.Zona;
        public double? Latitudine { get; init; } = s.Latitudine;
        public double? Longitudine { get; init; } = s.Longitudine;
    }
}
