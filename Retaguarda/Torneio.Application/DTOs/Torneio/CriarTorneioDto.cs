using System.ComponentModel.DataAnnotations;
using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Torneio;

public class CriarTorneioDto
{
    [Required(ErrorMessage = "O slug é obrigatório.")]
    public string Slug { get; init; } = null!;

    [Required(ErrorMessage = "O nome do torneio é obrigatório.")]
    public string NomeTorneio { get; init; } = null!;
    public DateTime? DataTorneio { get; init; }
    public string? Descricao { get; init; }
    public string? ObservacoesInternas { get; init; }

    public string? LogoUrl { get; set; }
    public TipoTorneio TipoTorneio { get; init; } = TipoTorneio.Pesca;

    [Required] public string LabelEquipe       { get; init; } = null!;
    [Required] public string LabelEquipePlural  { get; init; } = null!;
    [Required] public string LabelMembro        { get; init; } = null!;
    [Required] public string LabelMembroPlural  { get; init; } = null!;
    [Required] public string LabelSupervisor    { get; init; } = null!;
    [Required] public string LabelSupervisorPlural { get; init; } = null!;
    [Required] public string LabelItem          { get; init; } = null!;
    [Required] public string LabelItemPlural    { get; init; } = null!;
    [Required] public string LabelCaptura       { get; init; } = null!;
    [Required] public string LabelCapturaPlural { get; init; } = null!;
    [Required] public string MedidaCaptura      { get; init; } = null!;

    public bool UsarFatorMultiplicador { get; init; }
    public bool PermitirCapturaOffline { get; init; } = true;
    public bool ExibirModuloFinanceiro { get; init; } = true;
    public bool PermitirRegistroPublicoMembro { get; init; }
    public bool ExibirParticipantesPublicos { get; init; }
    public bool ExibirNaListaInicialPublica { get; init; } = true;
    public bool ExibirNaPesquisaPublica { get; init; } = true;
    public ModoSorteio ModoSorteio { get; init; }

    [Range(1, 100, ErrorMessage = "Informe entre 1 e 100 ganhadores.")]
    public int QtdGanhadores { get; init; } = 3;

    public bool PremiacaoPorEquipe { get; init; } = true;
    public bool PremiacaoPorMembro { get; init; } = true;
    public bool ApenasMaiorCapturaPorPescador { get; init; } = false;

    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor inválida. Use o formato #RRGGBB.")]
    public string? CorPrimaria { get; init; }
}
