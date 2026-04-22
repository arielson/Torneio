namespace Torneio.Application.DTOs.Financeiro;

public class FluxoFinanceiroLinhaDto
{
    public DateTime Data { get; init; }
    public decimal RecebimentosPrevistos { get; init; }
    public decimal PagamentosPrevistos { get; init; }
    public decimal SaldoDiario { get; init; }
    public decimal SaldoAcumulado { get; init; }
}
