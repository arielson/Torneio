using FluentValidation;
using Torneio.Application.DTOs.AdminTorneio;

namespace Torneio.Application.Validators;

public class CriarAdminTorneioValidator : AbstractValidator<CriarAdminTorneioDto>
{
    public CriarAdminTorneioValidator()
    {
        RuleFor(x => x.TorneioId).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Usuario).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Senha)
            .NotEmpty()
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");
    }
}
