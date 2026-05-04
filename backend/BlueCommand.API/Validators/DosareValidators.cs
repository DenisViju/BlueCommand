using BlueCommand.API.DTOs.Dosare;
using FluentValidation;

namespace BlueCommand.API.Validators;

public class CreateDosarRequestValidator : AbstractValidator<CreateDosarRequestDto>
{
    public CreateDosarRequestValidator()
    {
        RuleFor(x => x.NumarDosar).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Titlu).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SectieId).GreaterThan(0);
        RuleFor(x => x.AgentiIds).NotNull().Must(a => a.Count > 0).WithMessage("Cel putin un agent trebuie asignat");
    }
}

public class UpdateDosarAgentiRequestValidator : AbstractValidator<UpdateDosarAgentiRequestDto>
{
    public UpdateDosarAgentiRequestValidator()
    {
        RuleFor(x => x.AgentiIds).NotNull();
    }
}

