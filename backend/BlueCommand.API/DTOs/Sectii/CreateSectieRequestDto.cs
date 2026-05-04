namespace BlueCommand.API.DTOs.Sectii;

public class CreateSectieRequestDto
{
    public string Nume { get; set; } = null!;
    public string? Adresa { get; set; }
    public string? Zona { get; set; }
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }
}

