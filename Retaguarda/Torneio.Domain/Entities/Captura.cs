using Torneio.Domain.Enums;
using Torneio.Domain.ValueObjects;

namespace Torneio.Domain.Entities;

public class Captura
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid MembroId { get; private set; }
    public Guid EquipeId { get; private set; }
    public decimal TamanhoMedida { get; private set; }
    public string? FotoUrl { get; private set; }
    public DateTime DataHora { get; private set; }
    public bool PendenteSync { get; private set; }
    public OrigemCaptura Origem { get; private set; }
    public FonteFoto? FonteFoto { get; private set; }
    public bool Invalidada { get; private set; }
    public string? MotivoInvalidacao { get; private set; }

    // Navegação (carregada pelo EF Core quando necessário)
    public Item? Item { get; private set; }

    private Captura() { }

    public static Captura Criar(
        Guid torneioId,
        Guid itemId,
        Guid membroId,
        Guid equipeId,
        decimal tamanhoMedida,
        string? fotoUrl,
        DateTime dataHora,
        bool pendenteSync = false,
        OrigemCaptura origem = OrigemCaptura.App,
        FonteFoto? fonteFoto = null)
    {
        return new Captura
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            ItemId = itemId,
            MembroId = membroId,
            EquipeId = equipeId,
            TamanhoMedida = tamanhoMedida,
            FotoUrl = fotoUrl,
            DataHora = dataHora,
            PendenteSync = pendenteSync,
            Origem = origem,
            FonteFoto = fonteFoto,
            Invalidada = false
        };
    }

    public void Invalidar(string motivo)
    {
        Invalidada = true;
        MotivoInvalidacao = motivo;
    }

    public void Revalidar()
    {
        Invalidada = false;
        MotivoInvalidacao = null;
    }

    public void AlterarTamanho(decimal tamanhoMedida)
    {
        if (tamanhoMedida <= 0)
            throw new InvalidOperationException("O tamanho da captura deve ser maior que zero.");

        TamanhoMedida = tamanhoMedida;
    }

    public Pontuacao CalcularPontuacao()
    {
        if (Item is null)
            throw new InvalidOperationException("Item deve ser carregado para calcular a pontuação.");
        return Pontuacao.Calcular(TamanhoMedida, Item.FatorMultiplicador);
    }

    public void MarcarSincronizado() => PendenteSync = false;

    public void AtualizarFoto(string? fotoUrl) => FotoUrl = fotoUrl;

    public void EditarCompleto(decimal tamanhoMedida, string? fotoUrl, DateTime dataHora)
    {
        if (tamanhoMedida <= 0)
            throw new InvalidOperationException("O tamanho da captura deve ser maior que zero.");
        TamanhoMedida = tamanhoMedida;
        FotoUrl = fotoUrl;
        DataHora = dataHora;
    }
}
