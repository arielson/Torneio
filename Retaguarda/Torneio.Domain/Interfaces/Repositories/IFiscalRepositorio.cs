using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IFiscalRepositorio : IRepositorio<Fiscal>
{
    Task<Fiscal?> ObterPorUsuario(string usuario, Guid torneioId);
    Task<IEnumerable<Fiscal>> ListarPorTorneio(Guid torneioId);
    Task<Fiscal?> ObterComEquipes(Guid id);
}
