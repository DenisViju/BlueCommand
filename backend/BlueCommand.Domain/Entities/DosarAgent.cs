namespace BlueCommand.Domain.Entities;

public class DosarAgent
{
    public int DosarId { get; set; }
    public int UtilizatorId { get; set; }

    public Dosar Dosar { get; set; } = null!;
    public Utilizator Utilizator { get; set; } = null!;
}

