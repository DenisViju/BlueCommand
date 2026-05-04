namespace BlueCommand.Application.Interfaces;

public interface IAuditService
{
    Task LogActionAsync(int? userId, string actiune, string? detalii, string? ipAdresa, CancellationToken cancellationToken = default);
}

