namespace BlueCommand.Domain.Entities;

public class IstoricDosar
{
    public int Id { get; set; }
    public int DosarId { get; set; }
    public string CampModificat { get; set; } = null!;
    public string? ValoareVeche { get; set; }
    public string? ValoareNoua { get; set; }
    public int ModificatDe { get; set; }
    public DateTime ModificatLa { get; set; }

    public Dosar Dosar { get; set; } = null!;
    public Utilizator ModificatDeUtilizator { get; set; } = null!;
}

