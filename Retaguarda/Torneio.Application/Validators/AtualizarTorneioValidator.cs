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

        RuleFor(x => x.Descricao).MaximumLength(2000)
            .WithMessage("A descriÃ§Ã£o deve ter no mÃ¡ximo 2000 caracteres.");

        RuleFor(x => x.ObservacoesInternas).MaximumLength(4000)
            .WithMessage("As observaÃ§Ãµes internas devem ter no mÃ¡ximo 4000 caracteres.");

        RuleFor(x => x.ModoSorteio)
            .IsInEnum().WithMessage("Selecione um modo de sorteio.");
    }
}
