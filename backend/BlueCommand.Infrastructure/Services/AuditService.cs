using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Entities;
using BlueCommand.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly BlueCommandDbContext _db;

    public AuditService(BlueCommandDbContext db)
    {
        _db = db;
    }

    public async Task LogActionAsync(int? userId, string actiune, string? detalii, string? ipAdresa, CancellationToken cancellationToken = default)
    {
        _db.AuditLog.Add(new AuditLog
        {
            UtilizatorId = userId,
            Actiune = actiune,
            Detalii = detalii,
            IpAdresa = ipAdresa,
            CreatLa = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}

