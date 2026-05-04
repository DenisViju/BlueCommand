namespace BlueCommand.API.DTOs.Auth;

public class LoginResponseDto
{
    public required string Token { get; init; }
    public required UtilizatorAuthDto Utilizator { get; init; }
}

public class UtilizatorAuthDto
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public string? Nume { get; init; }
    public string? Prenume { get; init; }
    public required string Rol { get; init; }
    public int? SectieId { get; init; }
}

