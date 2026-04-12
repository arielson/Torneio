namespace Torneio.Application.DTOs.Fiscal;

public class FiscalDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid AnoTorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string? FotoUrl { get; init; }
    public string Usuario { get; init; } = null!;
}
