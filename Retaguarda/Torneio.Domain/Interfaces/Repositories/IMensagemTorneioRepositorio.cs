using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IMensagemTorneioRepositorio : IRepositorio<MensagemTorneio>
{
    Task<IEnumerable<MensagemTorneio>> ListarPorTorneio(Guid torneioId);
}
