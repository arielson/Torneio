namespace Torneio.Application.DTOs.Financeiro;

public class ProdutoExtraMembroDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid ProdutoExtraTorneioId { get; init; }
    public Guid MembroId { get; init; }
    public string NomeMembro { get; init; } = null!;
    public decimal Quantidade { get; init; }
    public decimal ValorCobrado { get; init; }
    public string? Observacao { get; init; }
    public bool Ativo { get; init; }
    public Guid? ParcelaId { get; init; }
    public bool Pago { get; init; }
    public DateTime? DataPagamento { get; init; }
    public bool Inadimplente { get; init; }
}
