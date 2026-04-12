using FluentValidation;
using Torneio.Application.DTOs.AnoTorneio;

namespace Torneio.Application.Validators;

public class CriarAnoTorneioValidator : AbstractValidator<CriarAnoTorneioDto>
{
    public CriarAnoTorneioValidator()
    {
        RuleFor(x => x.TorneioId)
            .NotEmpty().WithMessage("TorneioId é obrigatório.");

        RuleFor(x => x.Ano)
            .InclusiveBetween(2000, 2100).WithMessage("Ano deve estar entre 2000 e 2100.");
    }
}
