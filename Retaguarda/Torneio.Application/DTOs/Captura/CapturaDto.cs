using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Captura;

public class CapturaDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid ItemId { get; init; }
    public string NomeItem { get; init; } = null!;
    public Guid MembroId { get; init; }
    public string NomeMembro { get; init; } = null!;
    public Guid EquipeId { get; init; }
    public string NomeEquipe { get; init; } = null!;
    public decimal TamanhoMedida { get; init; }
    public decimal FatorMultiplicador { get; init; }
    public decimal Pontuacao { get; init; }
    public string? FotoUrl { get; init; }
    public DateTime DataHora { get; init; }
    public bool PendenteSync { get; init; }
    public OrigemCaptura Origem { get; init; }
    public FonteFoto? FonteFoto { get; init; }
    public bool Invalidada { get; init; }
    public string? MotivoInvalidacao { get; init; }
}
