using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ISorteioEquipeRepositorio : IRepositorio<SorteioEquipe>
{
    Task<IEnumerable<SorteioEquipe>> ListarPorTorneio(Guid torneioId);
    Task AdicionarLote(IEnumerable<SorteioEquipe> lista);
    Task RemoverPorTorneio(Guid torneioId);
}
