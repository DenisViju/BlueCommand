namespace BlueCommand.API.DTOs.Auth;

public class ChangePasswordRequestDto
{
    public string ParolaActuala { get; set; } = null!;
    public string ParolaNoua { get; set; } = null!;
}

