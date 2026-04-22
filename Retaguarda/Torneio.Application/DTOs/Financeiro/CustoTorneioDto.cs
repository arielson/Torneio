namespace Torneio.Application.DTOs.Financeiro;

public class CustoTorneioDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Categoria { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public decimal Quantidade { get; init; }
    public decimal ValorUnitario { get; init; }
    public decimal ValorTotal { get; init; }
    public string CategoriaLabel { get; init; } = null!;
    public DateTime? Vencimento { get; init; }
    public string? Responsavel { get; init; }
    public string? Observacao { get; init; }
    public bool DerivadoDaEmbarcacao { get; init; }
    public Guid? EquipeId { get; init; }
}
