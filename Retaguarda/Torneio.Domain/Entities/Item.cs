namespace Torneio.Domain.Entities;

public class Item
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid EspeciePeixeId { get; private set; }
    public EspeciePeixe Especie { get; private set; } = null!;
    public decimal? Comprimento { get; private set; }
    public decimal FatorMultiplicador { get; private set; }

    private Item() { }

    public static Item Criar(
        Guid torneioId,
        Guid especiePeixeId,
        decimal? comprimento,
        decimal fatorMultiplicador = 1.0m)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            EspeciePeixeId = especiePeixeId,
            Comprimento = comprimento,
            FatorMultiplicador = fatorMultiplicador
        };
    }

    public void Atualizar(Guid especiePeixeId, decimal? comprimento, decimal fatorMultiplicador)
    {
        EspeciePeixeId = especiePeixeId;
        Comprimento = comprimento;
        FatorMultiplicador = fatorMultiplicador;
    }
}
