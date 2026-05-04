namespace BlueCommand.Domain.Entities;

public class Utilizator
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public int? SectieId { get; set; }

    public string Username { get; set; } = null!;
    public string ParolaHash { get; set; } = null!;

    public string? Nume { get; set; }
    public string? Prenume { get; set; }
    public string? Grad { get; set; }

    public DateTime DataCreare { get; set; }
    public bool EsteActiv { get; set; } = true;

    public Rol Rol { get; set; } = null!;
    public Sectie? Sectie { get; set; }

    public List<DosarAgent> DosarAgenti { get; set; } = new();
    public List<Dosar> DosareCreate { get; set; } = new();
    public List<DocumentDosar> DocumenteIncarcate { get; set; } = new();
    public List<Raport> Rapoarte { get; set; } = new();
}

