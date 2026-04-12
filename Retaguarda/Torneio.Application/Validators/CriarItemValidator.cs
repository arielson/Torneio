using FluentValidation;
using Torneio.Application.DTOs.Item;

namespace Torneio.Application.Validators;

public class CriarItemValidator : AbstractValidator<CriarItemDto>
{
    public CriarItemValidator()
    {
        RuleFor(x => x.TorneioId).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Comprimento)
            .GreaterThan(0).WithMessage("Comprimento deve ser maior que zero.");
        RuleFor(x => x.FatorMultiplicador)
            .GreaterThan(0).WithMessage("Fator multiplicador deve ser maior que zero.");
    }
}
