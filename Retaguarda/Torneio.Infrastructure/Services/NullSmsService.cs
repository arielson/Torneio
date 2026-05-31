using Microsoft.Extensions.Logging;
using Torneio.Application.Services.Interfaces;

namespace Torneio.Infrastructure.Services;

public class NullSmsService : ISmsService
{
    private readonly ILogger<NullSmsService> _logger;

    public NullSmsService(ILogger<NullSmsService> logger) => _logger = logger;

    public Task EnviarCodigoAsync(string numero, string codigo)
    {
        _logger.LogWarning("[SMS desabilitado] Código {Codigo} → {Numero}", codigo, numero);
        return Task.CompletedTask;
    }

    public Task EnviarMensagemAsync(string numero, string mensagem)
    {
        _logger.LogWarning("[SMS desabilitado] {Numero} → {Mensagem}", numero, mensagem);
        return Task.CompletedTask;
    }
}
