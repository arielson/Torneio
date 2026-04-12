namespace Torneio.Application.DTOs.AnoTorneio;

public class AnoTorneioDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public int Ano { get; init; }
    public string Status { get; init; } = null!;
}
