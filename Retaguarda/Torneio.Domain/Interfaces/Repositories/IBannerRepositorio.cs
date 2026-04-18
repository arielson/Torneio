using Torneio.Domain.Entities;
namespace Torneio.Domain.Interfaces.Repositories;
public interface IBannerRepositorio : IRepositorio<Banner>
{
    Task<IEnumerable<Banner>> ListarAtivos();
    Task<Banner?> ObterComTorneio(Guid id);
}
