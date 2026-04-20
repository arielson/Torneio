using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IAdminTorneioRepositorio : IRepositorio<AdminTorneio>
{
    Task<AdminTorneio?> ObterPorUsuario(string usuario);
    Task<AdminTorneio?> ObterPorUsuario(string usuario, Guid torneioId);
    Task<IEnumerable<AdminTorneio>> ListarPorTorneio(Guid torneioId);
    Task<IEnumerable<AdminTorneio>> ListarPorUsuarioId(Guid usuarioId);
}
