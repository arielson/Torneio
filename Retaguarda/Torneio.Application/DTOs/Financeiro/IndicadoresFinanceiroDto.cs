namespace Torneio.Application.DTOs.Financeiro;

public class IndicadoresFinanceiroDto
{
    public int QuantidadeMembros { get; init; }
    public int QuantidadeEquipes { get; init; }
    public int QuantidadeAdministradores { get; init; }
    public decimal CustoTotalTorneio { get; init; }
    public decimal ValorPorMembro { get; init; }
    public decimal TaxaInscricaoValor { get; init; }
    public int QuantidadeParcelas { get; init; }
    public decimal ArrecadacaoPrevista { get; init; }
    public decimal ReceitaPrevista { get; init; }
    public decimal SaldoProjetado { get; init; }
    public int ParcelasInadimplentes { get; init; }
    public decimal ValorEmAberto { get; init; }
    public int EmbarcacoesConfirmadas { get; init; }
    public int QuantidadeCustosLancados { get; init; }
    public int QuantidadeProdutosExtras { get; init; }
    public decimal ReceitaExtrasPrevista { get; init; }
    public int QuantidadeDoacoes { get; init; }
    public decimal ReceitaDoacoesPatrocinadores { get; init; }
}
