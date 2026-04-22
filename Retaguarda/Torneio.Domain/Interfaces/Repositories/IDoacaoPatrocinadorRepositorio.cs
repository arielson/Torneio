using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IDoacaoPatrocinadorRepositorio : IRepositorio<DoacaoPatrocinador>
{
    Task<IEnumerable<DoacaoPatrocinador>> ListarPorTorneio(Guid torneioId);
}
