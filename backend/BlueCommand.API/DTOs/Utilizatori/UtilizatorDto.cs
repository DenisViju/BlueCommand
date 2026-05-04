namespace BlueCommand.API.DTOs.Utilizatori;

public class UtilizatorDto
{
    public int Id { get; init; }
    public required string Username { get; init; }
    public string? Nume { get; init; }
    public string? Prenume { get; init; }
    public string? Grad { get; init; }
    public required string Rol { get; init; }
    public int? SectieId { get; init; }
    public string? SectieNume { get; init; }
    public DateTime DataCreare { get; init; }
    public bool EsteActiv { get; init; }
}

