using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services.Options;

namespace Torneio.Infrastructure.Services;

public class TwilioSmsVerificacaoServico : ISmsVerificacaoServico
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioSmsVerificacaoServico> _logger;

    public TwilioSmsVerificacaoServico(
        IOptions<TwilioOptions> options,
        ILogger<TwilioSmsVerificacaoServico> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnviarCodigo(string celularE164)
    {
        ValidarConfiguracao();

        using var client = CriarCliente();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"Services/{_options.VerifyServiceSid}/Verifications");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = celularE164,
            ["Channel"] = "sms",
        });

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return;

        var erro = ExtrairMensagemErro(body) ?? "Nao foi possivel enviar o codigo por SMS.";
        _logger.LogWarning("Falha ao enviar SMS Twilio para {Celular}: {Erro}", celularE164, erro);
        throw new InvalidOperationException(erro);
    }

    public async Task<bool> ValidarCodigo(string celularE164, string codigo)
    {
        ValidarConfiguracao();

        using var client = CriarCliente();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"Services/{_options.VerifyServiceSid}/VerificationCheck");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = celularE164,
            ["Code"] = codigo,
        });

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var erro = ExtrairMensagemErro(body) ?? "Nao foi possivel validar o codigo informado.";
            _logger.LogWarning("Falha ao validar SMS Twilio para {Celular}: {Erro}", celularE164, erro);
            throw new InvalidOperationException(erro);
        }

        using var document = JsonDocument.Parse(body);
        return string.Equals(
            document.RootElement.GetProperty("status").GetString(),
            "approved",
            StringComparison.OrdinalIgnoreCase);
    }

    private void ValidarConfiguracao()
    {
        if (!_options.Enabled)
            throw new InvalidOperationException("O envio de SMS nao esta habilitado no servidor.");

        if (string.IsNullOrWhiteSpace(_options.AccountSid) ||
            string.IsNullOrWhiteSpace(_options.AuthToken) ||
            string.IsNullOrWhiteSpace(_options.VerifyServiceSid))
        {
            throw new InvalidOperationException("A configuracao do Twilio esta incompleta.");
        }
    }

    private HttpClient CriarCliente()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://verify.twilio.com/v2/")
        };

        var authBytes = Encoding.ASCII.GetBytes($"{_options.AccountSid}:{_options.AuthToken}");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        return client;
    }

    private static string? ExtrairMensagemErro(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("message", out var messageElement))
                return messageElement.GetString();
        }
        catch
        {
            return null;
        }

        return null;
    }
}
