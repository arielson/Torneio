namespace Torneio.Application.DTOs.Patrocinador;

public class PatrocinadorDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string FotoUrl { get; init; } = null!;
    public string? Instagram { get; init; }
    public string? Site { get; init; }
    public string? Zap { get; init; }
}
