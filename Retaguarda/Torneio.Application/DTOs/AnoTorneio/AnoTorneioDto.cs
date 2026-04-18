namespace Torneio.Application.DTOs.AnoTorneio;

public class AnoTorneioDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Titulo { get; init; } = null!;
    public string Status { get; init; } = null!;
}
