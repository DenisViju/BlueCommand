using BlueCommand.API.DTOs.Dashboard;
using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Constants;
using BlueCommand.Domain.Enums;
using BlueCommand.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "TotiUtilizatorii")]
public class DashboardController : ControllerBase
{
    private readonly BlueCommandDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(BlueCommandDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("statistici")]
    public async Task<ActionResult<DashboardStatsDto>> Statistici(CancellationToken cancellationToken)
    {
        var role = _currentUser.Role;
        var sectieId = _currentUser.SectieId;

        var dosare = _db.Dosare.AsNoTracking().AsQueryable();
        var utilizatori = _db.Utilizatori.AsNoTracking().Include(u => u.Rol).AsQueryable();

        if (role == RoluriDenumiri.AgentPolitie && sectieId.HasValue)
        {
            dosare = dosare.Where(d => d.SectieId == sectieId.Value);
            utilizatori = utilizatori.Where(u => u.SectieId == sectieId.Value);
        }

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var last30 = now.AddDays(-30);

        var totalDosare = await dosare.CountAsync(cancellationToken);
        var deschise = await dosare.CountAsync(d => d.Status == DosarStatus.DESCHIS, cancellationToken);
        var inLucru = await dosare.CountAsync(d => d.Status == DosarStatus.IN_LUCRU, cancellationToken);
        var inchise = await dosare.CountAsync(d => d.Status == DosarStatus.INCHIS, cancellationToken);

        var totalAgenti = await utilizatori.CountAsync(u => u.Rol.Denumire == RoluriDenumiri.AgentPolitie && u.EsteActiv, cancellationToken);
        var totalSectii = role == RoluriDenumiri.AgentPolitie ? (sectieId.HasValue ? 1 : 0) : await _db.Sectii.CountAsync(cancellationToken);

        var incidente30 = await dosare.CountAsync(d => d.DataIncident != null && d.DataIncident.Value >= last30, cancellationToken);

        return Ok(new DashboardStatsDto
        {
            TotalDosare = totalDosare,
            DosareDeschise = deschise,
            DosareInLucru = inLucru,
            DosareInchise = inchise,
            TotalAgenti = totalAgenti,
            TotalSectii = totalSectii,
            IncidentePeUltimele30Zile = incidente30
        });
    }
}

