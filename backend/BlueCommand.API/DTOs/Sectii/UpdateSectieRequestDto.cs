namespace BlueCommand.API.DTOs.Sectii;

public class UpdateSectieRequestDto
{
    public string? Nume { get; set; }
    public string? Adresa { get; set; }
    public string? Zona { get; set; }
    public double? Latitudine { get; set; }
    public double? Longitudine { get; set; }
}

