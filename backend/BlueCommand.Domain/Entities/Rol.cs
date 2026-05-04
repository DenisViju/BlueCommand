namespace BlueCommand.Domain.Entities;

public class Rol
{
    public int Id { get; set; }
    public string Denumire { get; set; } = null!;

    public List<Utilizator> Utilizatori { get; set; } = new();
}

