using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Torneio;

public class TorneioDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = null!;
    public string NomeTorneio { get; init; } = null!;
    public string? LogoUrl { get; init; }
    public bool Ativo { get; init; }
    public string Status { get; init; } = null!;
    public string LabelEquipe { get; init; } = null!;
    public string LabelEquipePlural { get; init; } = null!;
    public string LabelMembro { get; init; } = null!;
    public string LabelMembroPlural { get; init; } = null!;
    public string LabelSupervisor { get; init; } = null!;
    public string LabelSupervisorPlural { get; init; } = null!;
    public string LabelItem { get; init; } = null!;
    public string LabelItemPlural { get; init; } = null!;
    public string LabelCaptura { get; init; } = null!;
    public string LabelCapturaPlural { get; init; } = null!;
    public bool UsarFatorMultiplicador { get; init; }
    public string MedidaCaptura { get; init; } = null!;
    public bool PermitirCapturaOffline { get; init; }
    public string ModoSorteio { get; init; } = null!;
    public int QtdGanhadores { get; init; }
    public bool PremiacaoPorEquipe { get; init; }
    public bool PremiacaoPorMembro { get; init; }
    public TipoTorneio TipoTorneio { get; init; }
    public DateTime CriadoEm { get; init; }
    public string? CorPrimaria { get; init; }
}
