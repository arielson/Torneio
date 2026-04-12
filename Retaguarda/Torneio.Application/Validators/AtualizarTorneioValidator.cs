using FluentValidation;
using Torneio.Application.DTOs.Torneio;

namespace Torneio.Application.Validators;

public class AtualizarTorneioValidator : AbstractValidator<AtualizarTorneioDto>
{
    public AtualizarTorneioValidator()
    {
        RuleFor(x => x.NomeTorneio)
            .NotEmpty().WithMessage("Nome do torneio é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.ModoSorteio)
            .IsInEnum().WithMessage("Selecione um modo de sorteio.");
    }
}
