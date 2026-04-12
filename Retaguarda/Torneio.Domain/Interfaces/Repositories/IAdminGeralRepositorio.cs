using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IAdminGeralRepositorio : IRepositorio<AdminGeral>
{
    Task<AdminGeral?> ObterPorUsuario(string usuario);
}
