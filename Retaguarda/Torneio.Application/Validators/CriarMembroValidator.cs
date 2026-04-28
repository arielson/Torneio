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
        RuleFor(x => x.Usuario).MaximumLength(100);
        RuleFor(x => x.Senha)
            .MaximumLength(100)
            .MinimumLength(6).When(x => !string.IsNullOrWhiteSpace(x.Senha))
            .WithMessage("Senha deve ter no minimo 6 caracteres.");
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.Usuario) == string.IsNullOrWhiteSpace(x.Senha))
            .WithMessage("Informe usuario e senha juntos para habilitar o acesso do pescador.");
    }
}
