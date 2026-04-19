using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Captura;

public class RegistrarCapturaDto
{
    public Guid TorneioId { get; init; }
    public Guid ItemId { get; init; }
    public Guid MembroId { get; init; }
    public Guid EquipeId { get; init; }
    public decimal TamanhoMedida { get; init; }
    public string? FotoUrl { get; init; }
    public DateTime DataHora { get; init; }
    public bool PendenteSync { get; init; } = false;
    public OrigemCaptura Origem { get; init; } = OrigemCaptura.App;
    public FonteFoto? FonteFoto { get; init; }
}
