using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ISeguidorTorneioRepositorio : IRepositorio<SeguidorTorneio>
{
    Task<SeguidorTorneio?> ObterPorToken(Guid torneioId, string deviceToken);
    Task<IEnumerable<string>> ListarTokens(Guid torneioId);
    Task RemoverPorToken(Guid torneioId, string deviceToken);
}
