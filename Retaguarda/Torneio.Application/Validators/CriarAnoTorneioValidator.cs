using FluentValidation;
using Torneio.Application.DTOs.AnoTorneio;

namespace Torneio.Application.Validators;

public class CriarAnoTorneioValidator : AbstractValidator<CriarAnoTorneioDto>
{
    public CriarAnoTorneioValidator()
    {
        RuleFor(x => x.TorneioId)
            .NotEmpty().WithMessage("TorneioId é obrigatório.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("O título da edição é obrigatório.")
            .MinimumLength(2).WithMessage("O título deve ter pelo menos 2 caracteres.")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");
    }
}
