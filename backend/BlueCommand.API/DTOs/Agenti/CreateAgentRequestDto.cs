namespace BlueCommand.API.DTOs.Agenti;

public class CreateAgentRequestDto
{
    public string Username { get; set; } = null!;
    public string Parola { get; set; } = null!;
    public string Nume { get; set; } = null!;
    public string Prenume { get; set; } = null!;
    public string Grad { get; set; } = null!;
    public int SectieId { get; set; }
}

