using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IEquipeRepositorio : IRepositorio<Equipe>
{
    Task<IEnumerable<Equipe>> ListarTodos();
    Task<Equipe?> ObterPorFiscal(Guid fiscalId);
    Task<Equipe?> ObterComMembros(Guid id);
}
