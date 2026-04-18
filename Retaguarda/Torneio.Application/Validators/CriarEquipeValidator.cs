using FluentValidation;
using Torneio.Application.DTOs.Equipe;

namespace Torneio.Application.Validators;

public class CriarEquipeValidator : AbstractValidator<CriarEquipeDto>
{
    public CriarEquipeValidator()
    {
        RuleFor(x => x.TorneioId).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capitao).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FiscalId).NotEmpty();
        RuleFor(x => x.QtdVagas)
            .GreaterThan(0).WithMessage("Quantidade de vagas deve ser maior que zero.");
    }
}
