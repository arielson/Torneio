namespace Torneio.Domain.ValueObjects;

public sealed class Pontuacao
{
    public decimal Valor { get; }

    private Pontuacao(decimal valor)
    {
        Valor = valor;
    }

    public static Pontuacao Calcular(decimal tamanho, decimal fator)
    {
        return new Pontuacao(tamanho * fator);
    }

    public override string ToString() => Valor.ToString("F2");
}
