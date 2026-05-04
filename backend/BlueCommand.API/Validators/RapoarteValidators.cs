using BlueCommand.API.DTOs.Rapoarte;
using FluentValidation;

namespace BlueCommand.API.Validators;

public class GenerateRaportRequestValidator : AbstractValidator<GenerateRaportRequestDto>
{
    public GenerateRaportRequestValidator()
    {
        RuleFor(x => x.Tip).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DataStart).NotEmpty();
        RuleFor(x => x.DataEnd).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.DataStart <= x.DataEnd)
            .WithMessage("Data de început trebuie să fie înainte de data de sfârșit.");
    }
}

public class ExportRaportRequestValidator : AbstractValidator<ExportRaportRequestDto>
{
    public ExportRaportRequestValidator()
    {
        Include(new GenerateRaportRequestValidator());
        RuleFor(x => x.Format).NotEmpty();
    }
}
