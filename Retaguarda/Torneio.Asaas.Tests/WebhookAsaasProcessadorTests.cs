using Moq;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Infrastructure.Services;
using Xunit;

namespace Torneio.Asaas.Tests;

public class WebhookAsaasProcessadorTests
{
    private record Contexto(
        WebhookAsaasProcessador Processador,
        Mock<IWebhookEventoAsaasRepositorio> WebhookRepo,
        Mock<ICobrancaAsaasRepositorio> CobrancaRepo,
        Mock<IParcelaTorneioRepositorio> ParcelaRepo);

    private static Contexto Criar()
    {
        var webhookRepo = new Mock<IWebhookEventoAsaasRepositorio>();
        var cobrancaRepo = new Mock<ICobrancaAsaasRepositorio>();
        var parcelaRepo = new Mock<IParcelaTorneioRepositorio>();
        var tenantContext = new TenantContext();
        var calc = new CalculadoraPrevisaoCredito(32);

        webhookRepo.Setup(r => r.ExisteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var processador = new WebhookAsaasProcessador(
            webhookRepo.Object, cobrancaRepo.Object, parcelaRepo.Object,
            tenantContext, calc);

        return new Contexto(processador, webhookRepo, cobrancaRepo, parcelaRepo);
    }

    private static string Json(string id, string evt, string payId,
        string? payDate = null, string? estCreditDate = null, string? creditDate = null)
    {
        var pd = payDate is not null ? $@",""paymentDate"":""{payDate}""" : "";
        var ec = estCreditDate is not null ? $@",""estimatedCreditDate"":""{estCreditDate}""" : "";
        var cd = creditDate is not null ? $@",""creditDate"":""{creditDate}""" : "";
        return $@"{{""id"":""{id}"",""event"":""{evt}"",""payment"":{{""id"":""{payId}"",""value"":100.00{pd}{ec}{cd}}}}}";
    }

    private static CobrancaAsaas CriarCobranca(Guid parcelaId, string paymentId = "pay-001",
        FormaPagamentoAsaas? forma = null)
    {
        var c = CobrancaAsaas.Criar(
            torneioId: Guid.NewGuid(), membroId: Guid.NewGuid(),
            parcelaTorneioId: parcelaId, asaasPaymentId: paymentId,
            asaasCustomerId: null, asaasInvoiceUrl: null,
            valorOriginal: 100m, vencimento: DateTime.UtcNow.AddDays(30));
        if (forma.HasValue) c.AtualizarStatus(StatusCobrancaAsaas.Pendente, formaPagamento: forma);
        return c;
    }

    private static ParcelaTorneio CriarParcela(Guid torneioId, Guid membroId, bool pago = false)
    {
        var p = ParcelaTorneio.Criar(torneioId, membroId, TipoParcelaTorneio.TaxaInscricao, 1,
            "Parcela teste", 100m, DateTime.UtcNow.AddDays(30));
        if (pago) p.MarcarComoPago();
        return p;
    }

    // ── Idempotência ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_EventoJaExiste_RetornaSemAdicionarRegistro()
    {
        var ctx = Criar();
        ctx.WebhookRepo.Setup(r => r.ExisteAsync("evt-001")).ReturnsAsync(true);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_CONFIRMED", "pay-001"));

        ctx.WebhookRepo.Verify(r => r.Adicionar(It.IsAny<WebhookEventoAsaas>()), Times.Never);
    }

    // ── Payload inválido ──────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_PayloadNulo_ThrowsInvalidOperation()
    {
        var ctx = Criar();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Processador.ProcessarAsync("null"));
    }

    // ── PAYMENT_CONFIRMED ─────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_PaymentConfirmed_MarcaCobrancaConfirmadaEParcelaPaga()
    {
        var ctx = Criar();
        var parcelaId = Guid.NewGuid();
        var cobranca = CriarCobranca(parcelaId);
        var parcela = CriarParcela(cobranca.TorneioId, cobranca.MembroId);

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);
        ctx.ParcelaRepo.Setup(r => r.ObterPorId(parcelaId)).ReturnsAsync(parcela);

        await ctx.Processador.ProcessarAsync(
            Json("evt-001", "PAYMENT_CONFIRMED", "pay-001",
                payDate: "2026-05-27", estCreditDate: "2026-05-28"));

        Assert.Equal(StatusCobrancaAsaas.Confirmado, cobranca.Status);
        Assert.Equal(new DateTime(2026, 5, 28), cobranca.DataPrevisaoCredito);
        Assert.True(parcela.Pago);

        ctx.CobrancaRepo.Verify(r => r.Atualizar(cobranca), Times.Once);
        ctx.ParcelaRepo.Verify(r => r.Atualizar(parcela), Times.Once);
    }

    [Fact]
    public async Task ProcessarAsync_PaymentConfirmed_ParcelaJaPaga_NaoMarcaNovamente()
    {
        var ctx = Criar();
        var parcelaId = Guid.NewGuid();
        var cobranca = CriarCobranca(parcelaId);
        var parcela = CriarParcela(cobranca.TorneioId, cobranca.MembroId, pago: true);

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);
        ctx.ParcelaRepo.Setup(r => r.ObterPorId(parcelaId)).ReturnsAsync(parcela);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_CONFIRMED", "pay-001"));

        ctx.CobrancaRepo.Verify(r => r.Atualizar(cobranca), Times.Once);
        ctx.ParcelaRepo.Verify(r => r.Atualizar(It.IsAny<ParcelaTorneio>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarAsync_PaymentConfirmed_SemEstimatedCreditDate_UsaCalculadora()
    {
        var ctx = Criar();
        var parcelaId = Guid.NewGuid();
        var cobranca = CriarCobranca(parcelaId, forma: FormaPagamentoAsaas.Pix);

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);
        ctx.ParcelaRepo.Setup(r => r.ObterPorId(parcelaId)).ReturnsAsync((ParcelaTorneio?)null);

        // payDate = sexta-feira → previsão PIX = segunda-feira seguinte
        await ctx.Processador.ProcessarAsync(
            Json("evt-001", "PAYMENT_CONFIRMED", "pay-001", payDate: "2026-05-22"));

        Assert.Equal(StatusCobrancaAsaas.Confirmado, cobranca.Status);
        Assert.Equal(new DateTime(2026, 5, 25), cobranca.DataPrevisaoCredito); // segunda
    }

    // ── PAYMENT_RECEIVED ──────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_PaymentReceived_MarcaRecebidoEAtribuiCreditoEfetivo()
    {
        var ctx = Criar();
        var parcelaId = Guid.NewGuid();
        var cobranca = CriarCobranca(parcelaId);
        var parcela = CriarParcela(cobranca.TorneioId, cobranca.MembroId);

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);
        ctx.ParcelaRepo.Setup(r => r.ObterPorId(parcelaId)).ReturnsAsync(parcela);

        await ctx.Processador.ProcessarAsync(
            Json("evt-001", "PAYMENT_RECEIVED", "pay-001", creditDate: "2026-05-28"));

        Assert.Equal(StatusCobrancaAsaas.Recebido, cobranca.Status);
        Assert.Equal(new DateTime(2026, 5, 28), cobranca.DataCreditoEfetivo);
        Assert.True(parcela.Pago);
    }

    // ── PAYMENT_OVERDUE ───────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_PaymentOverdue_MarcaCobrancaVencida()
    {
        var ctx = Criar();
        var cobranca = CriarCobranca(Guid.NewGuid());

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_OVERDUE", "pay-001"));

        Assert.Equal(StatusCobrancaAsaas.Vencido, cobranca.Status);
        ctx.CobrancaRepo.Verify(r => r.Atualizar(cobranca), Times.Once);
    }

    // ── PAYMENT_REFUNDED ──────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_PaymentRefunded_MarcaEstornadoEDesmarcaParcela()
    {
        var ctx = Criar();
        var parcelaId = Guid.NewGuid();
        var cobranca = CriarCobranca(parcelaId);
        var parcela = CriarParcela(cobranca.TorneioId, cobranca.MembroId, pago: true);

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);
        ctx.ParcelaRepo.Setup(r => r.ObterPorId(parcelaId)).ReturnsAsync(parcela);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_REFUNDED", "pay-001"));

        Assert.Equal(StatusCobrancaAsaas.Estornado, cobranca.Status);
        Assert.False(parcela.Pago);
        ctx.ParcelaRepo.Verify(r => r.Atualizar(parcela), Times.Once);
    }

    [Fact]
    public async Task ProcessarAsync_PaymentRefunded_ParcelaNaoPaga_NaoDesmarca()
    {
        var ctx = Criar();
        var parcelaId = Guid.NewGuid();
        var cobranca = CriarCobranca(parcelaId);
        var parcela = CriarParcela(cobranca.TorneioId, cobranca.MembroId, pago: false);

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);
        ctx.ParcelaRepo.Setup(r => r.ObterPorId(parcelaId)).ReturnsAsync(parcela);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_REFUNDED", "pay-001"));

        Assert.Equal(StatusCobrancaAsaas.Estornado, cobranca.Status);
        ctx.ParcelaRepo.Verify(r => r.Atualizar(It.IsAny<ParcelaTorneio>()), Times.Never);
    }

    // ── PAYMENT_DELETED ───────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_PaymentDeleted_MarcaCobrancaExcluida()
    {
        var ctx = Criar();
        var cobranca = CriarCobranca(Guid.NewGuid());

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_DELETED", "pay-001"));

        Assert.Equal(StatusCobrancaAsaas.Excluido, cobranca.Status);
    }

    // ── PAYMENT_CREDIT_CARD_CAPTURE_REFUSED ───────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_CreditCardRefused_MarcaCobrancaRecusadaCartao()
    {
        var ctx = Criar();
        var cobranca = CriarCobranca(Guid.NewGuid(), forma: FormaPagamentoAsaas.CartaoCredito);

        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId("pay-001")).ReturnsAsync(cobranca);

        await ctx.Processador.ProcessarAsync(
            Json("evt-001", "PAYMENT_CREDIT_CARD_CAPTURE_REFUSED", "pay-001"));

        Assert.Equal(StatusCobrancaAsaas.RecusadoCartao, cobranca.Status);
    }

    // ── Cobrança não encontrada ───────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_CobrancaNaoEncontrada_EventoMarcadoProcessadoSemErro()
    {
        var ctx = Criar();
        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId(It.IsAny<string>()))
            .ReturnsAsync((CobrancaAsaas?)null);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_CONFIRMED", "pay-desconhecido"));

        ctx.WebhookRepo.Verify(r => r.Adicionar(It.IsAny<WebhookEventoAsaas>()), Times.Once);
        ctx.WebhookRepo.Verify(
            r => r.Atualizar(It.Is<WebhookEventoAsaas>(e => e.Processado && e.ErroProcessamento == null)),
            Times.Once);
    }

    // ── Erro interno ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessarAsync_ErroInterno_EventoMarcadoComErro()
    {
        var ctx = Criar();
        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("erro de banco"));

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_CONFIRMED", "pay-001"));

        ctx.WebhookRepo.Verify(
            r => r.Atualizar(It.Is<WebhookEventoAsaas>(e =>
                !e.Processado && e.ErroProcessamento == "erro de banco")),
            Times.Once);
    }

    // ── Evento marcado como processado no caminho feliz ───────────────────────

    [Fact]
    public async Task ProcessarAsync_Sucesso_EventoMarcadoComoProcessado()
    {
        var ctx = Criar();
        ctx.CobrancaRepo.Setup(r => r.ObterPorAsaasPaymentId(It.IsAny<string>()))
            .ReturnsAsync((CobrancaAsaas?)null);

        await ctx.Processador.ProcessarAsync(Json("evt-001", "PAYMENT_OVERDUE", "pay-001"));

        ctx.WebhookRepo.Verify(
            r => r.Atualizar(It.Is<WebhookEventoAsaas>(e => e.Processado)),
            Times.Once);
    }
}
