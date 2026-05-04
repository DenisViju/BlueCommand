namespace BlueCommand.API.DTOs.Agenti;

public class AgentDto
{
    public int Id { get; init; }
    public required string Username { get; init; }
    public string? Nume { get; init; }
    public string? Prenume { get; init; }
    public string? Grad { get; init; }
    public required int SectieId { get; init; }
    public required string SectieNume { get; init; }
    public bool EsteActiv { get; init; }
}

