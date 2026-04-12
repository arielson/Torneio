using FluentValidation;
using Torneio.Application.DTOs.Auth;

namespace Torneio.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Usuario).NotEmpty().WithMessage("Usuário é obrigatório.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
