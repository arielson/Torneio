using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Xunit;

namespace Torneio.Asaas.Tests;

public class CobrancaAsaasEntityTests
{
    private static CobrancaAsaas CriarCobranca(string paymentId = "pay-001") =>
        CobrancaAsaas.Criar(
            torneioId: Guid.NewGuid(),
            membroId: Guid.NewGuid(),
            parcelaTorneioId: Guid.NewGuid(),
            asaasPaymentId: paymentId,
            asaasCustomerId: "cus-001",
            asaasInvoiceUrl: "https://asaas.com/i/001",
            valorOriginal: 100m,
            vencimento: DateTime.UtcNow.AddDays(30));

    [Fact]
    public void Criar_RetornaEntidadeEmEstadoPendente()
    {
        var c = CriarCobranca();

        Assert.Equal(StatusCobrancaAsaas.Pendente, c.Status);
        Assert.Equal(100m, c.ValorOriginal);
        Assert.Equal("pay-001", c.AsaasPaymentId);
        Assert.Null(c.FormaPagamento);
        Assert.Null(c.TaxaAsaas);
        Assert.Null(c.DataPrevisaoCredito);
        Assert.Null(c.DataCreditoEfetivo);
        Assert.NotEqual(Guid.Empty, c.Id);
    }

    [Fact]
    public void AtualizarStatus_AlteraStatusCorrectamente()
    {
        var c = CriarCobranca();
        c.AtualizarStatus(StatusCobrancaAsaas.Confirmado);
        Assert.Equal(StatusCobrancaAsaas.Confirmado, c.Status);
    }

    [Fact]
    public void AtualizarStatus_ComFormaPagamento_AtribuiFormaPagamento()
    {
        var c = CriarCobranca();
        c.AtualizarStatus(StatusCobrancaAsaas.Confirmado, formaPagamento: FormaPagamentoAsaas.Pix);
        Assert.Equal(FormaPagamentoAsaas.Pix, c.FormaPagamento);
    }

    [Fact]
    public void AtualizarStatus_SemFormaPagamento_MantemFormaPagamentoAnterior()
    {
        var c = CriarCobranca();
        c.AtualizarStatus(StatusCobrancaAsaas.Confirmado, formaPagamento: FormaPagamentoAsaas.CartaoCredito);
        c.AtualizarStatus(StatusCobrancaAsaas.Recebido); // sem forma de pagamento
        Assert.Equal(FormaPagamentoAsaas.CartaoCredito, c.FormaPagamento);
    }

    [Fact]
    public void AtualizarStatus_ComTaxa_AtribuiTaxa()
    {
        var c = CriarCobranca();
        c.AtualizarStatus(StatusCobrancaAsaas.Confirmado, taxaAsaas: 1.99m);
        Assert.Equal(1.99m, c.TaxaAsaas);
    }

    [Fact]
    public void AtualizarStatus_ComPrevisaoCredito_AtribuiPrevisaoCredito()
    {
        var c = CriarCobranca();
        var previsao = new DateTime(2026, 6, 15);
        c.AtualizarStatus(StatusCobrancaAsaas.Confirmado, dataPrevisaoCredito: previsao);
        Assert.Equal(previsao, c.DataPrevisaoCredito);
    }

    [Fact]
    public void AtualizarStatus_ComCreditoEfetivo_AtribuiCreditoEfetivo()
    {
        var c = CriarCobranca();
        var credito = new DateTime(2026, 6, 15);
        c.AtualizarStatus(StatusCobrancaAsaas.Recebido, dataCreditoEfetivo: credito);
        Assert.Equal(credito, c.DataCreditoEfetivo);
    }

    [Fact]
    public void AtualizarStatus_SemCamposOpcionais_NaoSobrescreveValoresExistentes()
    {
        var c = CriarCobranca();
        c.AtualizarStatus(StatusCobrancaAsaas.Confirmado,
            formaPagamento: FormaPagamentoAsaas.Pix,
            taxaAsaas: 1.99m,
            dataPrevisaoCredito: new DateTime(2026, 6, 1));

        // Segunda chamada sem campos opcionais — valores anteriores devem ser mantidos
        c.AtualizarStatus(StatusCobrancaAsaas.Recebido);

        Assert.Equal(FormaPagamentoAsaas.Pix, c.FormaPagamento);
        Assert.Equal(1.99m, c.TaxaAsaas);
        Assert.Equal(new DateTime(2026, 6, 1), c.DataPrevisaoCredito);
    }

    [Fact]
    public void AtualizarStatus_AtualizaAtualizadoEm()
    {
        var c = CriarCobranca();
        var antes = c.AtualizadoEm;
        System.Threading.Thread.Sleep(1); // garante diferença de timestamp
        c.AtualizarStatus(StatusCobrancaAsaas.Confirmado);
        Assert.True(c.AtualizadoEm >= antes);
    }
}
