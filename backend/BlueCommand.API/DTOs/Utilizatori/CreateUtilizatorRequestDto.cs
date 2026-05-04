namespace BlueCommand.API.DTOs.Utilizatori;

public class CreateUtilizatorRequestDto
{
    public string Username { get; set; } = null!;
    public string Parola { get; set; } = null!;
    public string Nume { get; set; } = null!;
    public string Prenume { get; set; } = null!;
    public string? Grad { get; set; }
    public int RolId { get; set; }
    public int? SectieId { get; set; }
}

