using FluentValidation;
using Torneio.Application.DTOs.Patrocinador;

namespace Torneio.Application.Validators;

public class CriarPatrocinadorValidator : AbstractValidator<CriarPatrocinadorDto>
{
    public CriarPatrocinadorValidator()
    {
        RuleFor(x => x.TorneioId).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FotoUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Instagram).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Instagram));
        RuleFor(x => x.Site).MaximumLength(300).When(x => !string.IsNullOrWhiteSpace(x.Site));
        RuleFor(x => x.Zap).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Zap));
    }
}
