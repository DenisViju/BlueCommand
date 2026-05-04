namespace BlueCommand.API.DTOs.Dosare;

public class DosarDetailDto
{
    public int Id { get; init; }
    public required string NumarDosar { get; init; }
    public string? Titlu { get; init; }
    public string? Descriere { get; init; }
    public required string Status { get; init; }
    public string? TipIncident { get; init; }
    public DateOnly? DataIncident { get; init; }
    public int SectieId { get; init; }
    public required string SectieNume { get; init; }
    public int CreatDe { get; init; }
    public DateTime CreatLa { get; init; }
    public DateTime? ActualizatLa { get; init; }
    public required List<DosarAgentDto> Agenti { get; init; }
    public required List<DosarDocumentDto> Documente { get; init; }
    public required List<IstoricItemDto> Istoric { get; init; }
}

public class DosarAgentDto
{
    public int Id { get; init; }
    public required string Username { get; init; }
    public string? Nume { get; init; }
    public string? Prenume { get; init; }
    public string? Grad { get; init; }
}

public class DosarDocumentDto
{
    public int Id { get; init; }
    public required string NumeFisier { get; init; }
    public required string CaleFisier { get; init; }
    public long? MarimeBytes { get; init; }
    public int IncarcatDe { get; init; }
    public DateTime DataIncarcare { get; init; }
}

public class IstoricItemDto
{
    public int Id { get; init; }
    public required string CampModificat { get; init; }
    public string? ValoareVeche { get; init; }
    public string? ValoareNoua { get; init; }
    public int ModificatDe { get; init; }
    public DateTime ModificatLa { get; init; }
}

