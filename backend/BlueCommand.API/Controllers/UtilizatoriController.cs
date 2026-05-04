using BlueCommand.API.DTOs.Common;
using BlueCommand.API.DTOs.Utilizatori;
using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Entities;
using BlueCommand.Infrastructure.Data;
using BlueCommand.Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/utilizatori")]
public class UtilizatoriController : ControllerBase
{
    private readonly BlueCommandDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public UtilizatoriController(BlueCommandDbContext db, IPasswordHasher passwordHasher, IAuditService audit, ICurrentUserService currentUser)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _audit = audit;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<ActionResult<PagedResultDto<UtilizatorDto>>> GetAll([FromQuery] string? search, [FromQuery] string? rol, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;
        page = Math.Max(page, 1);

        var query = _db.Utilizatori.AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.Sectie)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                u.Username.Contains(search) ||
                (u.Nume != null && u.Nume.Contains(search)) ||
                (u.Prenume != null && u.Prenume.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(rol))
            query = query.Where(u => u.Rol.Denumire == rol);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UtilizatorDto
            {
                Id = u.Id,
                Username = u.Username,
                Nume = u.Nume,
                Prenume = u.Prenume,
                Grad = u.Grad,
                Rol = u.Rol.Denumire,
                SectieId = u.SectieId,
                SectieNume = u.Sectie != null ? u.Sectie.Nume : null,
                DataCreare = u.DataCreare,
                EsteActiv = u.EsteActiv
            })
            .ToListAsync(cancellationToken);

        return Ok(new PagedResultDto<UtilizatorDto> { Items = items, Total = total, Page = page, PageSize = pageSize });
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<ActionResult<UtilizatorDto>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var u = await _db.Utilizatori.AsNoTracking()
            .Include(x => x.Rol)
            .Include(x => x.Sectie)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (u is null) return NotFound();

        return Ok(new UtilizatorDto
        {
            Id = u.Id,
            Username = u.Username,
            Nume = u.Nume,
            Prenume = u.Prenume,
            Grad = u.Grad,
            Rol = u.Rol.Denumire,
            SectieId = u.SectieId,
            SectieNume = u.Sectie?.Nume,
            DataCreare = u.DataCreare,
            EsteActiv = u.EsteActiv
        });
    }

    [HttpGet("profil")]
    [Authorize(Policy = "TotiUtilizatorii")]
    public async Task<ActionResult<UtilizatorDto>> Profil(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized();

        var u = await _db.Utilizatori.AsNoTracking()
            .Include(x => x.Rol)
            .Include(x => x.Sectie)
            .FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);
        if (u is null) return Unauthorized();

        return Ok(new UtilizatorDto
        {
            Id = u.Id,
            Username = u.Username,
            Nume = u.Nume,
            Prenume = u.Prenume,
            Grad = u.Grad,
            Rol = u.Rol.Denumire,
            SectieId = u.SectieId,
            SectieNume = u.Sectie?.Nume,
            DataCreare = u.DataCreare,
            EsteActiv = u.EsteActiv
        });
    }

    [HttpPost]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Create([FromBody] CreateUtilizatorRequestDto request, CancellationToken cancellationToken)
    {
        var exists = await _db.Utilizatori.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (exists)
            return BadRequest(new { error = "Username deja utilizat" });

        var rol = await _db.Roluri.FirstOrDefaultAsync(r => r.Id == request.RolId, cancellationToken);
        if (rol is null)
            return BadRequest(new { error = "Rol invalid" });

        if (rol.Denumire == "AgentPolitie" && request.SectieId is null)
            return BadRequest(new { error = "Sectia este obligatorie pentru AgentPolitie" });

        if (request.SectieId.HasValue)
        {
            var sectieExists = await _db.Sectii.AnyAsync(s => s.Id == request.SectieId.Value, cancellationToken);
            if (!sectieExists)
                return BadRequest(new { error = "Sectie invalida" });
        }

        var user = new Utilizator
        {
            Username = request.Username,
            ParolaHash = _passwordHasher.Hash(request.Parola),
            Nume = request.Nume,
            Prenume = request.Prenume,
            Grad = request.Grad,
            RolId = request.RolId,
            SectieId = request.SectieId,
            DataCreare = DateTime.UtcNow,
            EsteActiv = true
        };

        _db.Utilizatori.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "CREATE_UTILIZATOR", $"Created user {user.Username}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { id = user.Id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUtilizatorRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _db.Utilizatori.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null) return NotFound();

        var original = new Snapshot(user);

        if (request.Nume is not null) user.Nume = request.Nume;
        if (request.Prenume is not null) user.Prenume = request.Prenume;
        if (request.Grad is not null) user.Grad = request.Grad;
        if (request.RolId is not null) user.RolId = request.RolId.Value;
        if (request.SectieId is not null) user.SectieId = request.SectieId;
        if (request.EsteActiv is not null) user.EsteActiv = request.EsteActiv.Value;

        await _db.SaveChangesAsync(cancellationToken);

        var updated = new Snapshot(user);
        var modifiedBy = _currentUser.UserId ?? user.Id;
        foreach (var (field, oldVal, newVal) in ChangeTracker.Diff(original, updated))
        {
            _db.IstoricUtilizatori.Add(ChangeTracker.ToIstoricUtilizator(user.Id, field, oldVal, newVal, modifiedBy));
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogActionAsync(_currentUser.UserId, "UPDATE_UTILIZATOR", $"Updated user {user.Username}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpPut("{id:int}/reseteaza-parola")]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> ResetPassword([FromRoute] int id, [FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _db.Utilizatori.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null) return NotFound();

        user.ParolaHash = _passwordHasher.Hash(request.ParolaNoua);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "RESET_PAROLA", $"Reset password for user {user.Username}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> SoftDelete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var user = await _db.Utilizatori.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null) return NotFound();

        user.EsteActiv = false;
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(_currentUser.UserId, "DEZACTIVARE_UTILIZATOR", $"Deactivated user {user.Username}", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    private sealed record Snapshot(Utilizator u)
    {
        public string? Nume { get; init; } = u.Nume;
        public string? Prenume { get; init; } = u.Prenume;
        public string? Grad { get; init; } = u.Grad;
        public int RolId { get; init; } = u.RolId;
        public int? SectieId { get; init; } = u.SectieId;
        public bool EsteActiv { get; init; } = u.EsteActiv;
    }
}

