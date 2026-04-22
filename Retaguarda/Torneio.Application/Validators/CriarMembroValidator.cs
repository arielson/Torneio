using FluentValidation;
using Torneio.Application.DTOs.Membro;

namespace Torneio.Application.Validators;

public class CriarMembroValidator : AbstractValidator<CriarMembroDto>
{
    public CriarMembroValidator()
    {
        RuleFor(x => x.TorneioId).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Celular).MaximumLength(30);
        RuleFor(x => x.TamanhoCamisa).MaximumLength(20);
    }
}
