using System.ComponentModel.DataAnnotations;
using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Torneio;

public class AtualizarTorneioDto
{
    [Required(ErrorMessage = "O nome do torneio é obrigatório.")]
    public string NomeTorneio { get; init; } = null!;

    public string? LogoUrl { get; init; }

    [Required(ErrorMessage = "O label de equipe é obrigatório.")]
    public string LabelEquipe { get; init; } = null!;

    [Required(ErrorMessage = "O label de membro é obrigatório.")]
    public string LabelMembro { get; init; } = null!;

    [Required(ErrorMessage = "O label de supervisor é obrigatório.")]
    public string LabelSupervisor { get; init; } = null!;

    [Required(ErrorMessage = "O label de item é obrigatório.")]
    public string LabelItem { get; init; } = null!;

    [Required(ErrorMessage = "O label de captura é obrigatório.")]
    public string LabelCaptura { get; init; } = null!;

    public bool UsarFatorMultiplicador { get; init; }

    [Required(ErrorMessage = "A unidade de medida é obrigatória.")]
    public string MedidaCaptura { get; init; } = null!;

    public bool PermitirCapturaOffline { get; init; }
    public ModoSorteio ModoSorteio { get; init; }
}
