namespace BlueCommand.API.DTOs.Sectii;

public class SectieDto
{
    public int Id { get; init; }
    public required string Nume { get; init; }
    public string? Adresa { get; init; }
    public string? Zona { get; init; }
    public double? Latitudine { get; init; }
    public double? Longitudine { get; init; }
    public DateTime CreatLa { get; init; }
}

