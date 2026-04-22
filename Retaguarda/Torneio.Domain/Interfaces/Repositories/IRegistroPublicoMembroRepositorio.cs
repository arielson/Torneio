using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IRegistroPublicoMembroRepositorio : IRepositorio<RegistroPublicoMembro>
{
    Task<RegistroPublicoMembro?> ObterUltimoPorCelular(Guid torneioId, string celularNormalizado);
}
