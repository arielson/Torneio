using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ISorteioGrupoRepositorio : IRepositorio<SorteioGrupo>
{
    Task<IEnumerable<SorteioGrupo>> ListarPorTorneio(Guid torneioId);
    Task AdicionarLote(IEnumerable<SorteioGrupo> lista);
    Task RemoverPorTorneio(Guid torneioId);
}
