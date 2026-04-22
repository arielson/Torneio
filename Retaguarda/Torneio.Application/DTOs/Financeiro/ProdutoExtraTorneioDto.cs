namespace Torneio.Application.DTOs.Financeiro;

public class ProdutoExtraTorneioDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string? Descricao { get; init; }
    public decimal Valor { get; init; }
    public bool Ativo { get; init; }
    public int QuantidadeAderidos { get; init; }
}
