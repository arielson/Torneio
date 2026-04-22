namespace Torneio.Application.DTOs.Financeiro;

public class DoacaoPatrocinadorDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid? PatrocinadorId { get; init; }
    public string NomePatrocinador { get; init; } = null!;
    public string Tipo { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public decimal? Quantidade { get; init; }
    public decimal? Valor { get; init; }
    public string? Observacao { get; init; }
    public DateTime DataDoacao { get; init; }
    public bool GeraReceita => string.Equals(Tipo, "Dinheiro", StringComparison.OrdinalIgnoreCase) && (Valor ?? 0) > 0;
}
