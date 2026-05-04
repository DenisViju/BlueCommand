namespace BlueCommand.API.DTOs.Dosare;

public class UpdateDosarRequestDto
{
    public string? Titlu { get; set; }
    public string? Descriere { get; set; }
    public string? TipIncident { get; set; }
    public DateOnly? DataIncident { get; set; }
    public int? SectieId { get; set; }
    public string? Status { get; set; }
}

