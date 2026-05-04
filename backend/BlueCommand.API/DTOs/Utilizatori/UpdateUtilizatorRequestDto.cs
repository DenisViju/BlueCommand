namespace BlueCommand.API.DTOs.Utilizatori;

public class UpdateUtilizatorRequestDto
{
    public string? Nume { get; set; }
    public string? Prenume { get; set; }
    public string? Grad { get; set; }
    public int? RolId { get; set; }
    public int? SectieId { get; set; }
    public bool? EsteActiv { get; set; }
}

