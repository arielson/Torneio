using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IMembroRepositorio : IRepositorio<Membro>
{
    Task<IEnumerable<Membro>> ListarPorTorneio(Guid torneioId);
    Task<IEnumerable<Membro>> ListarPorEquipe(Guid equipeId);
    Task<Membro?> ObterPorCelularNormalizado(Guid torneioId, string celularNormalizado);
    Task<Membro?> ObterPorUsuario(Guid torneioId, string usuario);
}
