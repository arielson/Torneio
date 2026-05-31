using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services.Options;

namespace Torneio.Infrastructure.Services;

public class TwilioSmsService : ISmsService
{
    private readonly SmsOptions _options;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IOptions<SmsOptions> options, ILogger<TwilioSmsService> logger)
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
        var cfg = _options.Twilio;
        if (string.IsNullOrWhiteSpace(cfg.AccountSid) || string.IsNullOrWhiteSpace(cfg.AuthToken))
            throw new InvalidOperationException("Credenciais Twilio não configuradas.");

        var destino = !string.IsNullOrWhiteSpace(_options.NumeroRedirecionamentoDev)
            ? _options.NumeroRedirecionamentoDev
            : numero;

        using var client = CriarCliente(cfg.AccountSid, cfg.AuthToken);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://api.twilio.com/2010-04-01/Accounts/{cfg.AccountSid}/Messages.json");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = destino,
            ["From"] = cfg.PhoneNumber,
            ["Body"] = mensagem
        });

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("SMS Twilio enviado para {Numero}", destino);
            return;
        }

        var erro = ExtrairErro(body) ?? response.ReasonPhrase ?? "Erro desconhecido";
        _logger.LogWarning("Falha Twilio para {Numero}: {Erro}", destino, erro);
        throw new InvalidOperationException($"Não foi possível enviar o SMS: {erro}");
    }

    private static HttpClient CriarCliente(string accountSid, string authToken)
    {
        var client = new HttpClient();
        var credenciais = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credenciais);
        return client;
    }

    private static string? ExtrairErro(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var m)) return m.GetString();
        }
        catch { }
        return null;
    }
}
