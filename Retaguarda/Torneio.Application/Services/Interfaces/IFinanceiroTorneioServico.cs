using Torneio.Application.DTOs.Financeiro;

namespace Torneio.Application.Services.Interfaces;

public interface IFinanceiroTorneioServico
{
    Task<TorneioFinanceiroConfigDto> ObterConfiguracao(Guid torneioId);
    Task AtualizarConfiguracao(Guid torneioId, AtualizarTorneioFinanceiroDto dto);
    Task SincronizarParcelas(Guid torneioId);
    Task SincronizarParcelas(Guid torneioId, IReadOnlyCollection<Guid> membroIds, bool somenteNovos = false);
    Task<IEnumerable<ParcelaTorneioDto>> ListarParcelas(
        Guid torneioId,
        Guid? membroId = null,
        bool somenteInadimplentes = false,
        bool somenteNaoPagas = false,
        string? tipoParcela = null);
    Task<ParcelaTorneioDto?> ObterParcela(Guid id);
    Task AtualizarParcela(Guid id, AtualizarParcelaTorneioDto dto);
    Task AtualizarPagamento(Guid id, AtualizarPagamentoParcelaDto dto);
    Task AtualizarComprovante(Guid id, string nomeArquivo, string url, string? contentType, string usuarioNome);
    Task<IndicadoresFinanceiroDto> ObterIndicadores(Guid torneioId);
    Task<RelatorioFinanceiroDto> ObterRelatorio(Guid torneioId);
    Task ValidarRemocaoMembro(Guid membroId);
    bool ConfiguracaoPossuiDados(TorneioFinanceiroConfigDto config);
}
