namespace Torneio.Domain.Entities;

public class Item
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? FotoUrl { get; private set; }
    public decimal Comprimento { get; private set; }
    public decimal FatorMultiplicador { get; private set; }

    private Item() { }

    public static Item Criar(
        Guid torneioId,
        string nome,
        decimal comprimento,
        decimal fatorMultiplicador = 1.0m,
        string? fotoUrl = null)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome,
            Comprimento = comprimento,
            FatorMultiplicador = fatorMultiplicador,
            FotoUrl = fotoUrl
        };
    }

    public void Atualizar(string nome, decimal comprimento, decimal fatorMultiplicador, string? fotoUrl = null)
    {
        Nome = nome;
        Comprimento = comprimento;
        FatorMultiplicador = fatorMultiplicador;
        if (fotoUrl != null) FotoUrl = fotoUrl;
    }
}
