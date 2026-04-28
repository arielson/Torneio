using FluentValidation;
using Torneio.Application.DTOs.Torneio;

namespace Torneio.Application.Validators;

public class CriarTorneioValidator : AbstractValidator<CriarTorneioDto>
{
    public CriarTorneioValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug é obrigatório.")
            .MaximumLength(100).WithMessage("Slug deve ter no máximo 100 caracteres.")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hífens.");

        RuleFor(x => x.NomeTorneio)
            .NotEmpty().WithMessage("Nome do torneio é obrigatório.")
            .MaximumLength(200).WithMessage("Nome do torneio deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Descricao).MaximumLength(2000)
            .WithMessage("A descriÃ§Ã£o deve ter no mÃ¡ximo 2000 caracteres.");

        RuleFor(x => x.ObservacoesInternas).MaximumLength(4000)
            .WithMessage("As observaÃ§Ãµes internas devem ter no mÃ¡ximo 4000 caracteres.");

        RuleFor(x => x.ModoSorteio)
            .IsInEnum().WithMessage("Selecione um modo de sorteio.");
    }
}
