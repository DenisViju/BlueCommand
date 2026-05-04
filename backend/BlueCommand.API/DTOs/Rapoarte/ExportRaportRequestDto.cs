namespace BlueCommand.API.DTOs.Rapoarte;

public class ExportRaportRequestDto : GenerateRaportRequestDto
{
    public string Format { get; set; } = null!;
}

