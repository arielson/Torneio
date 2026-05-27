using Torneio.Application.DTOs.Notificacao;

namespace Torneio.Application.Services.Interfaces;

public interface IMensagemTorneioServico
{
    Task<MensagemTorneioDto> EnviarAsync(string titulo, string corpo, string criadoPor);
    Task<IEnumerable<MensagemTorneioDto>> ListarAsync();
    Task RemoverAsync(Guid id);
}
