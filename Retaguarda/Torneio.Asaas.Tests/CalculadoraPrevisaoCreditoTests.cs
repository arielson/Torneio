using Torneio.Asaas;
using Xunit;

namespace Torneio.Asaas.Tests;

public class CalculadoraPrevisaoCreditoTests
{
    // PIX: próximo dia útil (pula sábado e domingo)

    [Fact]
    public void CalcularPrevisaoPix_DiaSemana_RetornaProximoDia()
    {
        var calc = new CalculadoraPrevisaoCredito(32);
        // terça → quarta
        var confirmacao = new DateTime(2026, 5, 26); // terça
        Assert.Equal(new DateTime(2026, 5, 27), calc.CalcularPrevisaoPix(confirmacao));
    }

    [Fact]
    public void CalcularPrevisaoPix_Sexta_RetornaSegunda()
    {
        var calc = new CalculadoraPrevisaoCredito(32);
        var confirmacao = new DateTime(2026, 5, 22); // sexta
        Assert.Equal(new DateTime(2026, 5, 25), calc.CalcularPrevisaoPix(confirmacao)); // segunda
    }

    [Fact]
    public void CalcularPrevisaoPix_Sabado_RetornaSegunda()
    {
        var calc = new CalculadoraPrevisaoCredito(32);
        var confirmacao = new DateTime(2026, 5, 23); // sábado
        Assert.Equal(new DateTime(2026, 5, 25), calc.CalcularPrevisaoPix(confirmacao)); // segunda
    }

    [Fact]
    public void CalcularPrevisaoPix_Domingo_RetornaSegunda()
    {
        var calc = new CalculadoraPrevisaoCredito(32);
        var confirmacao = new DateTime(2026, 5, 24); // domingo
        Assert.Equal(new DateTime(2026, 5, 25), calc.CalcularPrevisaoPix(confirmacao)); // segunda
    }

    [Fact]
    public void CalcularPrevisaoPix_IgnoraHorario_UsaSoData()
    {
        var calc = new CalculadoraPrevisaoCredito(32);
        var confirmacao = new DateTime(2026, 5, 26, 23, 59, 59); // terça às 23:59
        Assert.Equal(new DateTime(2026, 5, 27), calc.CalcularPrevisaoPix(confirmacao));
    }

    // Cartão: T + prazoDias dias corridos

    [Fact]
    public void CalcularPrevisaoCartao_32Dias_AdicionaDiasCorretos()
    {
        var calc = new CalculadoraPrevisaoCredito(32);
        var confirmacao = new DateTime(2026, 1, 1);
        Assert.Equal(new DateTime(2026, 2, 2), calc.CalcularPrevisaoCartao(confirmacao));
    }

    [Fact]
    public void CalcularPrevisaoCartao_30Dias_AdicionaDiasCorretos()
    {
        var calc = new CalculadoraPrevisaoCredito(30);
        var confirmacao = new DateTime(2026, 1, 1);
        Assert.Equal(new DateTime(2026, 1, 31), calc.CalcularPrevisaoCartao(confirmacao));
    }

    [Fact]
    public void CalcularPrevisaoCartao_NaoCaiFimDeSemana_PorqueDiasCorridos()
    {
        // Cartão usa dias corridos — pode cair no fim de semana, por design
        var calc = new CalculadoraPrevisaoCredito(32);
        var confirmacao = new DateTime(2026, 5, 1); // quinta
        var previsao = calc.CalcularPrevisaoCartao(confirmacao);
        Assert.Equal(new DateTime(2026, 6, 2), previsao); // terça — só confirma que não pula fds
    }

    [Fact]
    public void CalcularPrevisaoCartao_IgnoraHorario_UsaSoData()
    {
        var calc = new CalculadoraPrevisaoCredito(32);
        var confirmacao = new DateTime(2026, 1, 1, 23, 59, 59);
        Assert.Equal(new DateTime(2026, 2, 2), calc.CalcularPrevisaoCartao(confirmacao));
    }
}
