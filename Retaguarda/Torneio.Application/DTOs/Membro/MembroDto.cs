namespace Torneio.Application.DTOs.Membro;

public class MembroDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid AnoTorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string? FotoUrl { get; init; }
}
