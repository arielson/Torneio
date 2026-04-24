namespace Torneio.Domain.Entities;

public class ValorParcelaTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public int NumeroParcela { get; private set; }
    public decimal Valor { get; private set; }

    private ValorParcelaTorneio() { }

    public static ValorParcelaTorneio Criar(Guid torneioId, int numeroParcela, decimal valor) =>
        new() { Id = Guid.NewGuid(), TorneioId = torneioId, NumeroParcela = numeroParcela, Valor = valor };
}
