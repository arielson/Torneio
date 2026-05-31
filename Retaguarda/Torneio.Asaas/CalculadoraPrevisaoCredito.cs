namespace Torneio.Asaas;

public class CalculadoraPrevisaoCredito
{
    private readonly int _prazoCreditoCartaoDias;

    public CalculadoraPrevisaoCredito(int prazoCreditoCartaoDias)
    {
        _prazoCreditoCartaoDias = prazoCreditoCartaoDias;
    }

    // PIX: creditado no próximo dia útil (pula fim de semana, sem calendário de feriados)
    public DateTime CalcularPrevisaoPix(DateTime dataConfirmacao)
    {
        var data = dataConfirmacao.Date.AddDays(1);
        while (data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            data = data.AddDays(1);
        return data;
    }

    // Cartão: T + prazoDias dias corridos
    public DateTime CalcularPrevisaoCartao(DateTime dataConfirmacao)
        => dataConfirmacao.Date.AddDays(_prazoCreditoCartaoDias);
}
