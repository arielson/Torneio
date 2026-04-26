namespace Torneio.Application.DTOs.Membro;

public class PescaProPescadorDto
{
    public string Id { get; init; } = null!;
    public string Nome { get; init; } = null!;
    public string? Celular { get; init; }
    public string? FotoUrl { get; init; }
}
