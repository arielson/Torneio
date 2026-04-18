namespace Torneio.Application.DTOs.Premio;

public class PremioDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public int Posicao { get; init; }
    public string Descricao { get; init; } = null!;
}
