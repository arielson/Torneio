using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Torneio.Application.DTOs.Asaas;
using Torneio.Application.Services.Interfaces;
using Torneio.Asaas;
using Torneio.Asaas.Models.Webhooks;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Infrastructure.Services;

public class ConfiguracaoAsaasServico : IConfiguracaoAsaasServico
{
    private readonly IConfiguracaoAsaasRepositorio _repositorio;
    private readonly IAsaasClientFactory _clientFactory;
    private readonly AsaasOptions _options;
    private readonly IConfiguration _configuration;

    private static readonly List<string> EventosPadrao =
    [
        "PAYMENT_CONFIRMED",
        "PAYMENT_RECEIVED",
        "PAYMENT_OVERDUE",
        "PAYMENT_REFUNDED",
        "PAYMENT_DELETED",
        "PAYMENT_CREDIT_CARD_CAPTURE_REFUSED"
    ];

    public ConfiguracaoAsaasServico(
        IConfiguracaoAsaasRepositorio repositorio,
        IAsaasClientFactory clientFactory,
        IOptions<AsaasOptions> options,
        IConfiguration configuration)
    {
        _repositorio = repositorio;
        _clientFactory = clientFactory;
        _options = options.Value;
        _configuration = configuration;
    }

    public async Task<ConfiguracaoAsaasDto?> ObterPorTorneio(Guid torneioId)
    {
        var config = await _repositorio.ObterPorTorneioId(torneioId);
        return config is null ? null : Mapear(config);
    }

    public async Task Salvar(SalvarConfiguracaoAsaasDto dto)
    {
        var config = await _repositorio.ObterPorTorneioId(dto.TorneioId);
        if (config is null)
        {
            config = ConfiguracaoAsaasTorneio.Criar(dto.TorneioId);
            config.ConfigurarChave(dto.ChaveApiAsaas);
            config.AtualizarFormasPagamento(dto.AceitarPix, dto.AceitarCartaoCredito);
            await _repositorio.Adicionar(config);
        }
        else
        {
            config.ConfigurarChave(dto.ChaveApiAsaas);
            config.AtualizarFormasPagamento(dto.AceitarPix, dto.AceitarCartaoCredito);
            await _repositorio.Atualizar(config);
        }
    }

    public async Task Desativar(Guid torneioId)
    {
        var config = await _repositorio.ObterPorTorneioId(torneioId);
        if (config is null) return;
        config.Desativar();
        await _repositorio.Atualizar(config);
    }

    public async Task Reativar(Guid torneioId)
    {
        var config = await _repositorio.ObterPorTorneioId(torneioId);
        if (config is null) return;
        config.Reativar();
        await _repositorio.Atualizar(config);
    }

    public async Task RegistrarWebhook(Guid torneioId)
    {
        var config = await _repositorio.ObterPorTorneioId(torneioId);
        if (config is null || string.IsNullOrWhiteSpace(config.ChaveApiAsaas))
            throw new InvalidOperationException("Configuração Asaas não encontrada ou sem chave de API.");

        if (config.StatusChave != StatusChaveAsaas.Ativa)
            throw new InvalidOperationException("A integração Asaas está inativa. Reative antes de registrar o webhook.");

        var urlBase = _configuration["Plataforma:UrlBase"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Plataforma:UrlBase não configurada no appsettings.");

        var webhookUrl = $"{urlBase}/api/webhook/asaas";

        if (string.IsNullOrWhiteSpace(_options.WebhookAuthToken))
            throw new InvalidOperationException("Asaas:WebhookAuthToken não configurado.");

        var client = _clientFactory.Criar(config.ChaveApiAsaas);

        var eventos = _options.Webhook.EventosAssinados.Count > 0
            ? _options.Webhook.EventosAssinados
            : EventosPadrao;

        var request = new WebhookRequest
        {
            Name = "Torneio Pagamentos",
            Url = webhookUrl,
            Email = _options.WebhookEmail,
            Enabled = true,
            Interrupted = false,
            AuthToken = _options.WebhookAuthToken,
            ApiVersion = 3,
            SendType = "SEQUENTIALLY",
            Events = eventos
        };

        // Find existing webhook by URL — update if found, create if not
        var listagem = await client.Webhooks.ListAsync();
        var existente = listagem?.Data?.FirstOrDefault(w =>
            w.Url.Equals(webhookUrl, StringComparison.OrdinalIgnoreCase));

        if (existente is not null)
            await client.Webhooks.UpdateAsync(existente.Id, request);
        else
            await client.Webhooks.CreateAsync(request);
    }

    private static ConfiguracaoAsaasDto Mapear(ConfiguracaoAsaasTorneio config) =>
        new()
        {
            Id = config.Id,
            TorneioId = config.TorneioId,
            ChaveApiAsaas = config.ChaveApiAsaas,
            StatusChave = config.StatusChave.ToString(),
            AsaasAccountId = config.AsaasAccountId,
            AceitarPix = config.AceitarPix,
            AceitarCartaoCredito = config.AceitarCartaoCredito,
            DataAtivacao = config.DataAtivacao
        };
}
