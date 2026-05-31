using Newtonsoft.Json;
using Torneio.Application.Services.Interfaces;
using Torneio.Asaas;
using Torneio.Asaas.Models.Webhooks;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Infrastructure.Services;

public class WebhookAsaasProcessador : IWebhookAsaasProcessador
{
    private readonly IWebhookEventoAsaasRepositorio _webhookRepositorio;
    private readonly ICobrancaAsaasRepositorio _cobrancaRepositorio;
    private readonly IParcelaTorneioRepositorio _parcelaRepositorio;
    private readonly TenantContext _tenantContext;
    private readonly CalculadoraPrevisaoCredito _calculadoraPrevisao;

    public WebhookAsaasProcessador(
        IWebhookEventoAsaasRepositorio webhookRepositorio,
        ICobrancaAsaasRepositorio cobrancaRepositorio,
        IParcelaTorneioRepositorio parcelaRepositorio,
        TenantContext tenantContext,
        CalculadoraPrevisaoCredito calculadoraPrevisao)
    {
        _webhookRepositorio = webhookRepositorio;
        _cobrancaRepositorio = cobrancaRepositorio;
        _parcelaRepositorio = parcelaRepositorio;
        _tenantContext = tenantContext;
        _calculadoraPrevisao = calculadoraPrevisao;
    }

    public async Task ProcessarAsync(string payloadJson)
    {
        // Bypass all tenant query filters — webhook has no tenant context
        _tenantContext.DefinirAdminGeral();

        var payload = JsonConvert.DeserializeObject<AsaasWebhookPayload>(payloadJson)
            ?? throw new InvalidOperationException("Payload do webhook inválido.");

        var eventoId = payload.Id;
        var tipoEvento = payload.Event;
        var asaasPaymentId = payload.Payment?.Id;

        // Idempotency check
        if (await _webhookRepositorio.ExisteAsync(eventoId))
            return;

        var evento = WebhookEventoAsaas.Criar(eventoId, tipoEvento, asaasPaymentId, payloadJson);
        await _webhookRepositorio.Adicionar(evento);

        try
        {
            await ProcessarEvento(tipoEvento, asaasPaymentId, payload.Payment);
            evento.MarcarProcessado();
        }
        catch (Exception ex)
        {
            evento.MarcarErro(ex.Message);
        }

        await _webhookRepositorio.Atualizar(evento);
    }

    private async Task ProcessarEvento(
        string tipoEvento,
        string? asaasPaymentId,
        AsaasWebhookPaymentData? paymentData)
    {
        if (string.IsNullOrWhiteSpace(asaasPaymentId))
            return;

        var cobranca = await _cobrancaRepositorio.ObterPorAsaasPaymentId(asaasPaymentId);
        if (cobranca is null)
            return;

        switch (tipoEvento)
        {
            case "PAYMENT_CONFIRMED":
                await ProcessarConfirmado(cobranca, paymentData);
                break;

            case "PAYMENT_RECEIVED":
                await ProcessarRecebido(cobranca, paymentData);
                break;

            case "PAYMENT_OVERDUE":
                cobranca.AtualizarStatus(StatusCobrancaAsaas.Vencido);
                await _cobrancaRepositorio.Atualizar(cobranca);
                break;

            case "PAYMENT_REFUNDED":
                await ProcessarEstornado(cobranca);
                break;

            case "PAYMENT_DELETED":
                cobranca.AtualizarStatus(StatusCobrancaAsaas.Excluido);
                await _cobrancaRepositorio.Atualizar(cobranca);
                break;

            case "PAYMENT_CREDIT_CARD_CAPTURE_REFUSED":
                cobranca.AtualizarStatus(StatusCobrancaAsaas.RecusadoCartao);
                await _cobrancaRepositorio.Atualizar(cobranca);
                break;
        }
    }

    private async Task ProcessarConfirmado(CobrancaAsaas cobranca, AsaasWebhookPaymentData? paymentData)
    {
        DateTime? previsaoCredito = null;

        if (paymentData?.EstimatedCreditDate is not null
            && DateTime.TryParse(paymentData.EstimatedCreditDate, out var parsedPrevisao))
        {
            previsaoCredito = parsedPrevisao;
        }
        else
        {
            var dataConfirmacao = paymentData?.PaymentDate is not null
                && DateTime.TryParse(paymentData.PaymentDate, out var parsedPay)
                    ? parsedPay
                    : DateTime.UtcNow;

            previsaoCredito = cobranca.FormaPagamento == FormaPagamentoAsaas.CartaoCredito
                ? _calculadoraPrevisao.CalcularPrevisaoCartao(dataConfirmacao)
                : _calculadoraPrevisao.CalcularPrevisaoPix(dataConfirmacao);
        }

        cobranca.AtualizarStatus(
            StatusCobrancaAsaas.Confirmado,
            dataPrevisaoCredito: previsaoCredito);

        await _cobrancaRepositorio.Atualizar(cobranca);

        var parcela = await _parcelaRepositorio.ObterPorId(cobranca.ParcelaTorneioId);
        if (parcela is not null && !parcela.Pago)
        {
            var dataPagamento = paymentData?.PaymentDate is not null
                && DateTime.TryParse(paymentData.PaymentDate, out var parsedDate)
                    ? parsedDate
                    : DateTime.UtcNow;

            parcela.MarcarComoPago(dataPagamento);
            await _parcelaRepositorio.Atualizar(parcela);
        }
    }

    private async Task ProcessarRecebido(CobrancaAsaas cobranca, AsaasWebhookPaymentData? paymentData)
    {
        DateTime? creditoEfetivo = null;

        if (paymentData?.CreditDate is not null
            && DateTime.TryParse(paymentData.CreditDate, out var parsedCredit))
        {
            creditoEfetivo = parsedCredit;
        }

        cobranca.AtualizarStatus(
            StatusCobrancaAsaas.Recebido,
            dataCreditoEfetivo: creditoEfetivo);

        await _cobrancaRepositorio.Atualizar(cobranca);

        // Garante marcação caso PAYMENT_RECEIVED chegue sem PAYMENT_CONFIRMED anterior
        var parcela = await _parcelaRepositorio.ObterPorId(cobranca.ParcelaTorneioId);
        if (parcela is not null && !parcela.Pago)
        {
            parcela.MarcarComoPago(creditoEfetivo ?? DateTime.UtcNow);
            await _parcelaRepositorio.Atualizar(parcela);
        }
    }

    private async Task ProcessarEstornado(CobrancaAsaas cobranca)
    {
        cobranca.AtualizarStatus(StatusCobrancaAsaas.Estornado);
        await _cobrancaRepositorio.Atualizar(cobranca);

        var parcela = await _parcelaRepositorio.ObterPorId(cobranca.ParcelaTorneioId);
        if (parcela is not null && parcela.Pago)
        {
            parcela.DesmarcarPagamento();
            await _parcelaRepositorio.Atualizar(parcela);
        }
    }
}
