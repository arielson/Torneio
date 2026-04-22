using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IEquipeRepositorio : IRepositorio<Equipe>
{
    Task<IEnumerable<Equipe>> ListarPorTorneio(Guid torneioId);
    Task<IEnumerable<Equipe>> ListarPorFiscal(Guid torneioId, Guid fiscalId);
    Task<Equipe?> ObterComMembros(Guid id);
}
