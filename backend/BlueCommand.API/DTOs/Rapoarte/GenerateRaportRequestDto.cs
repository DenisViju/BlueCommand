namespace BlueCommand.API.DTOs.Rapoarte;

public class GenerateRaportRequestDto
{
    public string Tip { get; set; } = null!;
    public DateOnly DataStart { get; set; }
    public DateOnly DataEnd { get; set; }
    public int? SectieId { get; set; }
}

