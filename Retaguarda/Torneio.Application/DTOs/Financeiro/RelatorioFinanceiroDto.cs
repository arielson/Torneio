namespace Torneio.Application.DTOs.Financeiro;

public class RelatorioFinanceiroDto
{
    public IndicadoresFinanceiroDto Indicadores { get; init; } = new();
    public List<FluxoFinanceiroLinhaDto> FluxoCaixaProjetado { get; init; } = [];
    public List<ResumoFinanceiroPorTipoDto> ReceitasPorTipo { get; init; } = [];
    public List<ResumoFinanceiroPorCategoriaDto> CustosPorCategoria { get; init; } = [];
    public List<ParcelaTorneioDto> ProximosRecebimentosPendentes { get; init; } = [];
    public List<CustoTorneioDto> ProximosPagamentosPendentes { get; init; } = [];
}
