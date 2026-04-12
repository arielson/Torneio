using FluentValidation;
using Torneio.Application.DTOs.Captura;

namespace Torneio.Application.Validators;

public class RegistrarCapturaValidator : AbstractValidator<RegistrarCapturaDto>
{
    public RegistrarCapturaValidator()
    {
        RuleFor(x => x.TorneioId).NotEmpty();
        RuleFor(x => x.AnoTorneioId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.MembroId).NotEmpty();
        RuleFor(x => x.EquipeId).NotEmpty();
        RuleFor(x => x.TamanhoMedida)
            .GreaterThan(0).WithMessage("Tamanho da medida deve ser maior que zero.");
        RuleFor(x => x.FotoUrl).NotEmpty().WithMessage("Foto é obrigatória.");
        RuleFor(x => x.DataHora).NotEmpty();
    }
}
