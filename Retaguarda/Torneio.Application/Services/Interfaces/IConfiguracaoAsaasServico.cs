using Torneio.Application.DTOs.Asaas;

namespace Torneio.Application.Services.Interfaces;

public interface IConfiguracaoAsaasServico
{
    Task<ConfiguracaoAsaasDto?> ObterPorTorneio(Guid torneioId);
    Task Salvar(SalvarConfiguracaoAsaasDto dto);
    Task Desativar(Guid torneioId);
    Task Reativar(Guid torneioId);
    Task RegistrarWebhook(Guid torneioId);
}
