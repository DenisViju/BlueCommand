using BlueCommand.API.DTOs.Auth;
using FluentValidation;

namespace BlueCommand.API.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Parola).NotEmpty().MinimumLength(4);
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.ParolaActuala).NotEmpty();
        RuleFor(x => x.ParolaNoua).NotEmpty().MinimumLength(8);
    }
}

