using BlueCommand.API.DTOs.Sectii;
using FluentValidation;

namespace BlueCommand.API.Validators;

public class CreateSectieRequestValidator : AbstractValidator<CreateSectieRequestDto>
{
    public CreateSectieRequestValidator()
    {
        RuleFor(x => x.Nume).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Adresa).MaximumLength(200).When(x => x.Adresa is not null);
        RuleFor(x => x.Zona).MaximumLength(100).When(x => x.Zona is not null);
    }
}

public class UpdateSectieRequestValidator : AbstractValidator<UpdateSectieRequestDto>
{
    public UpdateSectieRequestValidator()
    {
        RuleFor(x => x.Nume).MaximumLength(100).When(x => x.Nume is not null);
        RuleFor(x => x.Adresa).MaximumLength(200).When(x => x.Adresa is not null);
        RuleFor(x => x.Zona).MaximumLength(100).When(x => x.Zona is not null);
    }
}

