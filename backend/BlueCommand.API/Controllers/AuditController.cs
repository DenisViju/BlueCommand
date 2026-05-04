using BlueCommand.API.DTOs.Common;
using BlueCommand.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Policy = "AdministratorOnly")]
public class AuditController : ControllerBase
{
    private readonly BlueCommandDbContext _db;

    public AuditController(BlueCommandDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<object>>> GetAll(
        [FromQuery] int? utilizatorId,
        [FromQuery] DateTime? dataStart,
        [FromQuery] DateTime? dataEnd,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;
        page = Math.Max(page, 1);

        var q = _db.AuditLog.AsNoTracking().AsQueryable();
        if (utilizatorId.HasValue) q = q.Where(a => a.UtilizatorId == utilizatorId.Value);
        if (dataStart.HasValue) q = q.Where(a => a.CreatLa >= dataStart.Value);
        if (dataEnd.HasValue) q = q.Where(a => a.CreatLa <= dataEnd.Value);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(a => a.CreatLa)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new { a.Id, a.UtilizatorId, a.Actiune, a.Detalii, a.IpAdresa, a.CreatLa })
            .ToListAsync(cancellationToken);

        return Ok(new PagedResultDto<object> { Items = items, Total = total, Page = page, PageSize = pageSize });
    }
}

