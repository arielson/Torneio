using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class AnoTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public int Ano { get; private set; }
    public StatusAnoTorneio Status { get; private set; }

    private AnoTorneio() { }

    public static AnoTorneio Criar(Guid torneioId, int ano)
    {
        return new AnoTorneio
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Ano = ano,
            Status = StatusAnoTorneio.Aberto
        };
    }

    public void Liberar()
    {
        if (Status != StatusAnoTorneio.Aberto)
            throw new InvalidOperationException("Somente anos com status Aberto podem ser liberados.");
        Status = StatusAnoTorneio.Liberado;
    }

    public void Finalizar()
    {
        if (Status != StatusAnoTorneio.Liberado)
            throw new InvalidOperationException("Somente anos com status Liberado podem ser finalizados.");
        Status = StatusAnoTorneio.Finalizado;
    }

    public void Reabrir()
    {
        if (Status == StatusAnoTorneio.Aberto)
            throw new InvalidOperationException("O ano já está aberto.");
        Status = StatusAnoTorneio.Aberto;
    }
}
