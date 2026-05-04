using BlueCommand.API.DTOs.Agenti;
using FluentValidation;

namespace BlueCommand.API.Validators;

public class CreateAgentRequestValidator : AbstractValidator<CreateAgentRequestDto>
{
    public CreateAgentRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Parola).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Nume).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prenume).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Grad).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SectieId).GreaterThan(0);
    }
}

public class UpdateAgentRequestValidator : AbstractValidator<UpdateAgentRequestDto>
{
    public UpdateAgentRequestValidator()
    {
        RuleFor(x => x.Nume).MaximumLength(100).When(x => x.Nume is not null);
        RuleFor(x => x.Prenume).MaximumLength(100).When(x => x.Prenume is not null);
        RuleFor(x => x.Grad).MaximumLength(100).When(x => x.Grad is not null);
    }
}

