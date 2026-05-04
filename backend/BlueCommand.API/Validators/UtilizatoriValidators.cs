using BlueCommand.API.DTOs.Utilizatori;
using FluentValidation;

namespace BlueCommand.API.Validators;

public class CreateUtilizatorRequestValidator : AbstractValidator<CreateUtilizatorRequestDto>
{
    public CreateUtilizatorRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Parola).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Nume).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prenume).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RolId).GreaterThan(0);
    }
}

public class UpdateUtilizatorRequestValidator : AbstractValidator<UpdateUtilizatorRequestDto>
{
    public UpdateUtilizatorRequestValidator()
    {
        RuleFor(x => x.Nume).MaximumLength(100).When(x => x.Nume is not null);
        RuleFor(x => x.Prenume).MaximumLength(100).When(x => x.Prenume is not null);
        RuleFor(x => x.Grad).MaximumLength(100).When(x => x.Grad is not null);
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.ParolaNoua).NotEmpty().MinimumLength(8);
    }
}

