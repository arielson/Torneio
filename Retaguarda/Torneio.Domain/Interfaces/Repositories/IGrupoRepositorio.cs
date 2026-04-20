using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IGrupoRepositorio : IRepositorio<Grupo>
{
    Task<IEnumerable<Grupo>> ListarComMembros();
    Task<Grupo?> ObterComMembros(Guid id);
    Task<bool> MembroJaEmGrupo(Guid membroId, Guid? excluirGrupoId = null);
    Task AdicionarMembro(GrupoMembro membro);
    Task RemoverMembro(Guid grupoMembroId);
}
