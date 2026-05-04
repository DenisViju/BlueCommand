using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Constants;
using BlueCommand.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.API.Middleware;

public class UploadsAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public UploadsAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, BlueCommandDbContext db, ICurrentUserService currentUser)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        // /uploads/{dosarId}/{filename}
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !string.Equals(segments[0], "uploads", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (!int.TryParse(segments[1], out var dosarId))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var dosar = await db.Dosare.AsNoTracking()
            .Include(d => d.DosarAgenti)
            .FirstOrDefaultAsync(d => d.Id == dosarId);

        if (dosar is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var role = currentUser.Role;
        if (string.IsNullOrWhiteSpace(role))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var userId = currentUser.UserId ?? -1;
        var sectieId = currentUser.SectieId;

        var allowed = role switch
        {
            RoluriDenumiri.Administrator => true,
            RoluriDenumiri.SefInspectorat => true,
            RoluriDenumiri.AgentPolitie =>
                (sectieId.HasValue && dosar.SectieId == sectieId.Value) ||
                dosar.DosarAgenti.Any(da => da.UtilizatorId == userId),
            _ => false
        };

        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }
}

