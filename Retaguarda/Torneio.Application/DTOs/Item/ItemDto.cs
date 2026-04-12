namespace Torneio.Application.DTOs.Item;

public class ItemDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string? FotoUrl { get; init; }
    public decimal Comprimento { get; init; }
    public decimal FatorMultiplicador { get; init; }
}
