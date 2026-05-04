namespace BlueCommand.API.DTOs.Dosare;

public class DosarListDto
{
    public int Id { get; init; }
    public required string NumarDosar { get; init; }
    public string? Titlu { get; init; }
    public string? TipIncident { get; init; }
    public DateOnly? DataIncident { get; init; }
    public required string Status { get; init; }
    public required int SectieId { get; init; }
    public required string SectieNume { get; init; }
    public DateTime CreatLa { get; init; }
}

