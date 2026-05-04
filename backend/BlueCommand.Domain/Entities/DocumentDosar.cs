namespace BlueCommand.Domain.Entities;

public class DocumentDosar
{
    public int Id { get; set; }
    public int DosarId { get; set; }
    public string NumeFisier { get; set; } = null!;
    public string CaleFisier { get; set; } = null!;
    public long? MarimeBytes { get; set; }
    public int IncarcatDe { get; set; }
    public DateTime DataIncarcare { get; set; }

    public Dosar Dosar { get; set; } = null!;
    public Utilizator IncarcatDeUtilizator { get; set; } = null!;
}

