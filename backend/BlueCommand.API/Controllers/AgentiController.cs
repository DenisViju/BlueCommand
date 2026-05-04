using BlueCommand.API.DTOs.Agenti;
using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Constants;
using BlueCommand.Domain.Entities;
using BlueCommand.Domain.Enums;
using BlueCommand.Infrastructure.Data;
using BlueCommand.Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/agenti")]
[Authorize(Policy = "SefSauAdmin")]
public class AgentiController : ControllerBase
{
    private readonly BlueCommandDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public AgentiController(BlueCommandDbContext db, IPasswordHasher passwordHasher, IAuditService audit, ICurrentUserService currentUser)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _audit = audit;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<AgentDto>>> GetAll([FromQuery] string? search, [FromQuery] int? sectieId, [FromQuery] string? grad, CancellationToken cancellationToken)
    {
        var query = _db.Utilizatori.AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.Sectie)
            .Where(u => u.Rol.Denumire == RoluriDenumiri.AgentPolitie)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                (u.Nume != null && u.Nume.Contains(search)) ||
                (u.Prenume != null && u.Prenume.Contains(search)) ||
                (u.Username != null && u.Username.Contains(search)));
        }

        if (sectieId.HasValue)
            query = query.Where(u => u.SectieId == sectieId.Value);

        if (!string.IsNullOrWhiteSpace(grad))
            query = query.Where(u => u.Grad != null && u.Grad.Contains(grad));

        var items = await query.OrderBy(u => u.Id)
            .Select(u => new AgentDto
            {
                Id = u.Id,
                Username = u.Username,
                Nume = u.Nume,
                Prenume = u.Prenume,
                Grad = u.Grad,
                SectieId = u.SectieId ?? 0,
                SectieNume = u.Sectie != null ? u.Sectie.Nume : "-",
                EsteActiv = u.EsteActiv
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var agent = await _db.Utilizatori.AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.Sectie)
            .FirstOrDefaultAsync(u => u.Id == id && u.Rol.Denumire == RoluriDenumiri.AgentPolitie, cancellationToken);
        if (agent is null) return NotFound();

        var openCases = await _db.DosarAgenti.AsNoTracking()
            .Where(da => da.UtilizatorId == id)
            .Select(da => da.Dosar)
            .Where(d => d.Status != DosarStatus.INCHIS)
            .OrderByDescending(d => d.CreatLa)
            .Select(d => new { d.Id, d.NumarDosar, d.Titlu, Status = d.Status.ToString(), d.SectieId })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            agent = new AgentDto
            {
                Id = agent.Id,
                Username = agent.Username,
                Nume = agent.Nume,
                Prenume = agent.Prenume,
                Grad = agent.Grad,
                SectieId = agent.SectieId ?? 0,
                SectieNume = agent.Sectie?.Nume ?? "-",
                EsteActiv = agent.EsteActiv
            },
            dosareActive = openCases
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAgentRequestDto request, CancellationToken cancellationToken)
    {
        if (await _db.Utilizatori.AnyAsync(u => u.Username == request.Username, cancellationToken))
            return BadRequest(new { error = "Username deja utilizat" });

        var sectie = await _db.Sectii.FirstOrDefaultAsync(s => s.Id == request.SectieId, cancellationToken);
        if (sectie is null)
            return BadRequest(new { error = "Sectie invalida" });

        var rolAgent = await _db.Roluri.FirstAsync(r => r.Denumire == RoluriDenumiri.AgentPolitie, cancellationToken);

        var user = new Utilizator
        {
            Username = request.Username,
            ParolaHash = _passwordHasher.Hash(request.Parola),
            Nume = request.Nume,
            Prenume = request.Prenume,
            Grad = request.Grad,
            SectieId = request.SectieId,
            RolId = rolAgent.Id,
            DataCreare = DateTime.UtcNow,
            EsteActiv = true
        };

        _db.Utilizatori.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "CREATE_AGENT", $"Created agent {user.Username}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { id = user.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAgentRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _db.Utilizatori
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id && u.Rol.Denumire == RoluriDenumiri.AgentPolitie, cancellationToken);

        if (user is null) return NotFound();

        var original = new Snapshot(user);

        if (request.Nume is not null) user.Nume = request.Nume;
        if (request.Prenume is not null) user.Prenume = request.Prenume;
        if (request.Grad is not null) user.Grad = request.Grad;
        if (request.SectieId is not null) user.SectieId = request.SectieId;

        await _db.SaveChangesAsync(cancellationToken);

        var updated = new Snapshot(user);
        var modifiedBy = _currentUser.UserId ?? user.Id;
        foreach (var (field, oldVal, newVal) in ChangeTracker.Diff(original, updated))
            _db.IstoricUtilizatori.Add(ChangeTracker.ToIstoricUtilizator(user.Id, field, oldVal, newVal, modifiedBy));

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogActionAsync(_currentUser.UserId, "UPDATE_AGENT", $"Updated agent {user.Username}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var user = await _db.Utilizatori
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id && u.Rol.Denumire == RoluriDenumiri.AgentPolitie, cancellationToken);

        if (user is null) return NotFound();

        var hasActiveCases = await _db.DosarAgenti
            .AnyAsync(da => da.UtilizatorId == id && da.Dosar.Status != DosarStatus.INCHIS, cancellationToken);
        if (hasActiveCases)
            return BadRequest(new { error = "Agentul are dosare active. Rezolvati dosarele inainte de stergere." });

        user.EsteActiv = false;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "DEACTIVATE_AGENT", $"Deactivated agent {user.Username}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpGet("{id:int}/istoric")]
    public async Task<IActionResult> Istoric([FromRoute] int id, CancellationToken cancellationToken)
    {
        var items = await _db.IstoricUtilizatori.AsNoTracking()
            .Where(i => i.UtilizatorId == id)
            .OrderByDescending(i => i.ModificatLa)
            .Select(i => new { i.Id, i.CampModificat, i.ValoareVeche, i.ValoareNoua, i.ModificatDe, i.ModificatLa })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    private sealed record Snapshot(Utilizator u)
    {
        public string? Nume { get; init; } = u.Nume;
        public string? Prenume { get; init; } = u.Prenume;
        public string? Grad { get; init; } = u.Grad;
        public int? SectieId { get; init; } = u.SectieId;
    }
}

