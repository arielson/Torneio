namespace Torneio.Application.DTOs.Financeiro;

public class ResumoFinanceiroPorTipoDto
{
    public string Chave { get; init; } = null!;
    public string Label { get; init; } = null!;
    public int Quantidade { get; init; }
    public decimal Total { get; init; }
    public decimal Pago { get; init; }
    public decimal EmAberto { get; init; }
}
