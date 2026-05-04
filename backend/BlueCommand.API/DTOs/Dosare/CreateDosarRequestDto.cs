namespace BlueCommand.API.DTOs.Dosare;

public class CreateDosarRequestDto
{
    public string NumarDosar { get; set; } = null!;
    public string Titlu { get; set; } = null!;
    public string? Descriere { get; set; }
    public string? TipIncident { get; set; }
    public DateOnly? DataIncident { get; set; }
    public int SectieId { get; set; }
    public List<int> AgentiIds { get; set; } = new();
}

