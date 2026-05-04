namespace BlueCommand.Domain.Entities;

public class Raport
{
    public int Id { get; set; }
    public int UtilizatorId { get; set; }
    public string Tip { get; set; } = null!;
    public string? FiltruPerioada { get; set; }
    public DateTime DataGenerare { get; set; }
    public string? CaleFisier { get; set; }

    public Utilizator Utilizator { get; set; } = null!;
}

