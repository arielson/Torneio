using System.ComponentModel.DataAnnotations;
using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Torneio;

public class AtualizarTorneioDto
{
    [Required(ErrorMessage = "O nome do torneio é obrigatório.")]
    public string NomeTorneio { get; init; } = null!;

    public string? LogoUrl { get; set; }

    [Required(ErrorMessage = "O label de equipe é obrigatório.")]
    public string LabelEquipe { get; init; } = null!;

    [Required(ErrorMessage = "O plural de equipe é obrigatório.")]
    public string LabelEquipePlural { get; init; } = null!;

    [Required(ErrorMessage = "O label de membro é obrigatório.")]
    public string LabelMembro { get; init; } = null!;

    [Required(ErrorMessage = "O plural de membro é obrigatório.")]
    public string LabelMembroPlural { get; init; } = null!;

    [Required(ErrorMessage = "O label de supervisor é obrigatório.")]
    public string LabelSupervisor { get; init; } = null!;

    [Required(ErrorMessage = "O plural de supervisor é obrigatório.")]
    public string LabelSupervisorPlural { get; init; } = null!;

    [Required(ErrorMessage = "O label de item é obrigatório.")]
    public string LabelItem { get; init; } = null!;

    [Required(ErrorMessage = "O plural de item é obrigatório.")]
    public string LabelItemPlural { get; init; } = null!;

    [Required(ErrorMessage = "O label de captura é obrigatório.")]
    public string LabelCaptura { get; init; } = null!;

    [Required(ErrorMessage = "O plural de captura é obrigatório.")]
    public string LabelCapturaPlural { get; init; } = null!;

    public bool UsarFatorMultiplicador { get; init; }

    [Required(ErrorMessage = "A unidade de medida é obrigatória.")]
    public string MedidaCaptura { get; init; } = null!;

    public bool PermitirCapturaOffline { get; init; }
    public bool ExibirModuloFinanceiro { get; init; } = true;
    public bool PermitirRegistroPublicoMembro { get; init; }
    public ModoSorteio ModoSorteio { get; init; }

    [System.ComponentModel.DataAnnotations.Range(1, 100, ErrorMessage = "Informe entre 1 e 100 ganhadores.")]
    public int QtdGanhadores { get; init; } = 3;

    public bool PremiacaoPorEquipe { get; init; } = true;
    public bool PremiacaoPorMembro { get; init; } = false;

    [System.ComponentModel.DataAnnotations.RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor inválida. Use o formato #RRGGBB.")]
    public string? CorPrimaria { get; init; }
}
