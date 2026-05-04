namespace BlueCommand.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int? UtilizatorId { get; set; }
    public string Actiune { get; set; } = null!;
    public string? Detalii { get; set; }
    public string? IpAdresa { get; set; }
    public DateTime CreatLa { get; set; }

    public Utilizator? Utilizator { get; set; }
}

