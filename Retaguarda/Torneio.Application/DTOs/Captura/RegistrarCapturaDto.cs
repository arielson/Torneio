namespace Torneio.Application.DTOs.Captura;

public class RegistrarCapturaDto
{
    public Guid TorneioId { get; init; }
    public Guid ItemId { get; init; }
    public Guid MembroId { get; init; }
    public Guid EquipeId { get; init; }
    public decimal TamanhoMedida { get; init; }
    public string FotoUrl { get; init; } = null!;
    public DateTime DataHora { get; init; }
    public bool PendenteSync { get; init; } = false;
}
