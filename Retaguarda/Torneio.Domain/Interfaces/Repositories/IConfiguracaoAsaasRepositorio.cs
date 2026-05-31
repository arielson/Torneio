using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface IConfiguracaoAsaasRepositorio
{
    Task<ConfiguracaoAsaasTorneio?> ObterPorTorneioId(Guid torneioId);
    Task Adicionar(ConfiguracaoAsaasTorneio config);
    Task Atualizar(ConfiguracaoAsaasTorneio config);
}
