using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services.Options;

namespace Torneio.Infrastructure.Services;

public class KingSmsService : ISmsService
{
    private readonly SmsOptions _options;
    private readonly ILogger<KingSmsService> _logger;

    public KingSmsService(IOptions<SmsOptions> options, ILogger<KingSmsService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task EnviarCodigoAsync(string numero, string codigo)
    {
        var mensagem = string.Format(_options.MensagemCodigo, codigo);
        return EnviarAsync(numero, mensagem);
    }

    public Task EnviarMensagemAsync(string numero, string mensagem) =>
        EnviarAsync(numero, mensagem);

    private async Task EnviarAsync(string numero, string mensagem)
    {
        var cfg = _options.KingSms;
        if (string.IsNullOrWhiteSpace(cfg.Login) || string.IsNullOrWhiteSpace(cfg.Token))
            throw new InvalidOperationException("Credenciais KingSMS não configuradas.");

        var destino = !string.IsNullOrWhiteSpace(_options.NumeroRedirecionamentoDev)
            ? _options.NumeroRedirecionamentoDev
            : numero;

        // KingSMS espera apenas dígitos (sem +)
        var destinoDigitos = new string(destino.Where(char.IsDigit).ToArray());

        var url = $"https://painel.kingsms.com.br/kingsms/api.php" +
                  $"?acao=sendsms&login={Uri.EscapeDataString(cfg.Login)}" +
                  $"&token={Uri.EscapeDataString(cfg.Token)}" +
                  $"&numero={destinoDigitos}" +
                  $"&msg={Uri.EscapeDataString(mensagem)}";

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        using var response = await client.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(body);
            var status = doc.RootElement.GetProperty("status").GetString() ?? "";
            if (status.Equals("success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("SMS KingSMS enviado para {Numero}", destinoDigitos);
                return;
            }

            var cause = doc.RootElement.TryGetProperty("cause", out var c) ? c.GetString() : body;
            _logger.LogWarning("Falha KingSMS para {Numero}: {Causa}", destinoDigitos, cause);
            throw new InvalidOperationException($"Não foi possível enviar o SMS: {cause}");
        }
        catch (JsonException)
        {
            _logger.LogWarning("Resposta KingSMS inválida para {Numero}: {Body}", destinoDigitos, body);
            throw new InvalidOperationException("Resposta inválida do serviço de SMS.");
        }
    }
}
