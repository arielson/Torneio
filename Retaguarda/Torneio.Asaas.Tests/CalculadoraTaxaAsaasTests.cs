using Torneio.Asaas;
using Xunit;

namespace Torneio.Asaas.Tests;

public class CalculadoraTaxaAsaasTests
{
    private static CalculadoraTaxaAsaas CriarCalculadora(
        decimal pix = 1.99m,
        decimal cartaoFixo = 0.49m,
        decimal cartaoPercentual = 2.99m,
        bool promocaoAtiva = false,
        decimal cartaoPercentualPromocional = 1.99m)
    {
        return new CalculadoraTaxaAsaas(new AsaasTaxasOptions
        {
            Pix = pix,
            CartaoFixo = cartaoFixo,
            CartaoPercentual = cartaoPercentual,
            PromocaoAtiva = promocaoAtiva,
            CartaoPercentualPromocional = cartaoPercentualPromocional
        });
    }

    // PIX

    [Theory]
    [InlineData(50.00)]
    [InlineData(100.00)]
    [InlineData(1000.00)]
    public void CalcularTaxaPix_SempreRetornaTaxaFlat(decimal valor)
    {
        var calc = CriarCalculadora(pix: 1.99m);
        Assert.Equal(1.99m, calc.CalcularTaxaPix(valor));
    }

    [Fact]
    public void CalcularValorLiquidoPix_DeduziTaxaFlat()
    {
        var calc = CriarCalculadora(pix: 1.99m);
        Assert.Equal(98.01m, calc.CalcularValorLiquidoPix(100.00m));
    }

    // Cartão — sem promoção

    [Fact]
    public void CalcularTaxaCartao_SemPromocao_UsaPercentualNormal()
    {
        var calc = CriarCalculadora(cartaoFixo: 0.49m, cartaoPercentual: 2.99m);
        // 100 * 2.99% = 2.99 + 0.49 = 3.48
        Assert.Equal(3.48m, calc.CalcularTaxaCartao(100.00m));
    }

    [Fact]
    public void CalcularTaxaCartao_SemPromocao_ArredondaDuasCasas()
    {
        var calc = CriarCalculadora(cartaoFixo: 0.49m, cartaoPercentual: 2.99m);
        // 10.34 * 2.99% = 0.309166... arredonda → 0.31 + 0.49 = 0.80
        // (truncar daria 0.30 + 0.49 = 0.79 — verifica que usa Round, não Truncate)
        Assert.Equal(0.80m, calc.CalcularTaxaCartao(10.34m));
    }

    [Fact]
    public void CalcularTaxaCartao_PromocaoAtiva_UsaPercentualPromocional()
    {
        var calc = CriarCalculadora(
            cartaoFixo: 0.49m,
            cartaoPercentual: 2.99m,
            promocaoAtiva: true,
            cartaoPercentualPromocional: 1.99m);
        // 100 * 1.99% = 1.99 + 0.49 = 2.48
        Assert.Equal(2.48m, calc.CalcularTaxaCartao(100.00m, usarPromocao: true));
    }

    [Fact]
    public void CalcularTaxaCartao_PromocaoInativa_IgnoraPercentualPromocional()
    {
        var calc = CriarCalculadora(
            cartaoFixo: 0.49m,
            cartaoPercentual: 2.99m,
            promocaoAtiva: false,
            cartaoPercentualPromocional: 1.99m);
        // Mesmo passando usarPromocao=true, promoção está desativada — usa percentual normal
        Assert.Equal(3.48m, calc.CalcularTaxaCartao(100.00m, usarPromocao: true));
    }

    [Fact]
    public void CalcularValorLiquidoCartao_DeduziTaxaCompleta()
    {
        var calc = CriarCalculadora(cartaoFixo: 0.49m, cartaoPercentual: 2.99m);
        // taxa = 3.48, líquido = 100 - 3.48 = 96.52
        Assert.Equal(96.52m, calc.CalcularValorLiquidoCartao(100.00m));
    }

    [Fact]
    public void CalcularValorLiquidoCartao_ComPromocao_DeduziTaxaPromocional()
    {
        var calc = CriarCalculadora(
            cartaoFixo: 0.49m,
            cartaoPercentual: 2.99m,
            promocaoAtiva: true,
            cartaoPercentualPromocional: 1.99m);
        // taxa = 2.48, líquido = 100 - 2.48 = 97.52
        Assert.Equal(97.52m, calc.CalcularValorLiquidoCartao(100.00m, usarPromocao: true));
    }
}
