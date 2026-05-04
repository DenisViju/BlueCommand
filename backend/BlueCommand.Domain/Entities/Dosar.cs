using BlueCommand.Domain.Enums;

namespace BlueCommand.Domain.Entities;

public class Dosar
{
    public int Id { get; set; }
    public string NumarDosar { get; set; } = null!;
    public string? Titlu { get; set; }
    public string? Descriere { get; set; }
    public DosarStatus Status { get; set; } = DosarStatus.DESCHIS;
    public string? TipIncident { get; set; }
    public DateOnly? DataIncident { get; set; }

    public int SectieId { get; set; }
    public int CreatDe { get; set; }
    public DateTime CreatLa { get; set; }
    public DateTime? ActualizatLa { get; set; }

    public Sectie Sectie { get; set; } = null!;
    public Utilizator CreatDeUtilizator { get; set; } = null!;

    public List<DosarAgent> DosarAgenti { get; set; } = new();
    public List<DocumentDosar> Documente { get; set; } = new();
    public List<IstoricDosar> Istoric { get; set; } = new();
}

