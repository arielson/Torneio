using FluentValidation;
using Torneio.Application.DTOs.AdminGeral;

namespace Torneio.Application.Validators;

public class CriarAdminGeralValidator : AbstractValidator<CriarAdminGeralDto>
{
    public CriarAdminGeralValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Usuario).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");
    }
}
