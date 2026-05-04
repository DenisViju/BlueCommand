namespace BlueCommand.Domain.Entities;

public class Sectie
{
    public int Id { get; set; }
    public string Nume { get; set; } = null!;
    public string? Adresa { get; set; }
    public string? Zona { get; set; }
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }
    public DateTime CreatLa { get; set; }

    public List<Utilizator> Utilizatori { get; set; } = new();
    public List<Dosar> Dosare { get; set; } = new();
    public List<IstoricSectie> Istoric { get; set; } = new();
}

