using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ICobrancaAsaasRepositorio
{
    Task<CobrancaAsaas?> ObterPorParcelaId(Guid parcelaTorneioId);
    Task<CobrancaAsaas?> ObterPorAsaasPaymentId(string asaasPaymentId);
    Task<IEnumerable<CobrancaAsaas>> ListarPorMembro(Guid torneioId, Guid membroId);
    Task Adicionar(CobrancaAsaas cobranca);
    Task Atualizar(CobrancaAsaas cobranca);
}
