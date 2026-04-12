using Torneio.Domain.ValueObjects;

namespace Torneio.Domain.Entities;

public class Captura
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid AnoTorneioId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid MembroId { get; private set; }
    public Guid EquipeId { get; private set; }
    public decimal TamanhoMedida { get; private set; }
    public string FotoUrl { get; private set; } = null!;
    public DateTime DataHora { get; private set; }
    public bool PendenteSync { get; private set; }

    // Navegação (carregada pelo EF Core quando necessário)
    public Item? Item { get; private set; }

    private Captura() { }

    public static Captura Criar(
        Guid torneioId,
        Guid anoTorneioId,
        Guid itemId,
        Guid membroId,
        Guid equipeId,
        decimal tamanhoMedida,
        string fotoUrl,
        DateTime dataHora,
        bool pendenteSync = false)
    {
        return new Captura
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            AnoTorneioId = anoTorneioId,
            ItemId = itemId,
            MembroId = membroId,
            EquipeId = equipeId,
            TamanhoMedida = tamanhoMedida,
            FotoUrl = fotoUrl,
            DataHora = dataHora,
            PendenteSync = pendenteSync
        };
    }

    public Pontuacao CalcularPontuacao()
    {
        if (Item is null)
            throw new InvalidOperationException("Item deve ser carregado para calcular a pontuação.");
        return Pontuacao.Calcular(TamanhoMedida, Item.FatorMultiplicador);
    }

    public void MarcarSincronizado() => PendenteSync = false;

    public void AtualizarFoto(string fotoUrl) => FotoUrl = fotoUrl;
}
