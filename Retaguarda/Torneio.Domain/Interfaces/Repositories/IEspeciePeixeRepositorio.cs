using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IEspeciePeixeRepositorio : IRepositorio<EspeciePeixe>
{
    Task<IEnumerable<EspeciePeixe>> ListarTodas();
}
