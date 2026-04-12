using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IEquipeRepositorio : IRepositorio<Equipe>
{
    Task<IEnumerable<Equipe>> ListarPorAnoTorneio(Guid anoTorneioId);
    Task<Equipe?> ObterPorFiscal(Guid fiscalId, Guid anoTorneioId);
    Task<Equipe?> ObterComMembros(Guid id);
}
