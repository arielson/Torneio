using Torneio.Domain.Entities;
using Xunit;

namespace Torneio.Asaas.Tests;

public class WebhookEventoAsaasTests
{
    [Fact]
    public void Criar_RetornaEntidadeComCamposCorretos()
    {
        var antes = DateTime.UtcNow;
        var evento = WebhookEventoAsaas.Criar("evt-001", "PAYMENT_CONFIRMED", "pay-001", "{\"id\":\"evt-001\"}");
        var depois = DateTime.UtcNow;

        Assert.Equal("evt-001", evento.EventoId);
        Assert.Equal("PAYMENT_CONFIRMED", evento.TipoEvento);
        Assert.Equal("pay-001", evento.AsaasPaymentId);
        Assert.False(evento.Processado);
        Assert.Null(evento.ErroProcessamento);
        Assert.Null(evento.ProcessadoEm);
        Assert.InRange(evento.RecebidoEm, antes, depois);
        Assert.NotEqual(Guid.Empty, evento.Id);
    }

    [Fact]
    public void Criar_SemPaymentId_DefineAsNulo()
    {
        var evento = WebhookEventoAsaas.Criar("evt-002", "PAYMENT_OVERDUE", null, "{}");
        Assert.Null(evento.AsaasPaymentId);
    }

    [Fact]
    public void MarcarProcessado_SetaProcessadoTrueELimpaErro()
    {
        var evento = WebhookEventoAsaas.Criar("evt-003", "PAYMENT_CONFIRMED", "pay-001", "{}");
        evento.MarcarErro("erro anterior");

        var antes = DateTime.UtcNow;
        evento.MarcarProcessado();
        var depois = DateTime.UtcNow;

        Assert.True(evento.Processado);
        Assert.Null(evento.ErroProcessamento);
        Assert.NotNull(evento.ProcessadoEm);
        Assert.InRange(evento.ProcessadoEm!.Value, antes, depois);
    }

    [Fact]
    public void MarcarErro_SetaProcessadoFalseERegistraErro()
    {
        var evento = WebhookEventoAsaas.Criar("evt-004", "PAYMENT_CONFIRMED", "pay-001", "{}");

        var antes = DateTime.UtcNow;
        evento.MarcarErro("cobrança não encontrada");
        var depois = DateTime.UtcNow;

        Assert.False(evento.Processado);
        Assert.Equal("cobrança não encontrada", evento.ErroProcessamento);
        Assert.NotNull(evento.ProcessadoEm);
        Assert.InRange(evento.ProcessadoEm!.Value, antes, depois);
    }

    [Fact]
    public void MarcarProcessado_AposaErro_ResetaEstado()
    {
        var evento = WebhookEventoAsaas.Criar("evt-005", "PAYMENT_CONFIRMED", "pay-001", "{}");
        evento.MarcarErro("falha temporária");
        evento.MarcarProcessado();

        Assert.True(evento.Processado);
        Assert.Null(evento.ErroProcessamento);
    }
}
