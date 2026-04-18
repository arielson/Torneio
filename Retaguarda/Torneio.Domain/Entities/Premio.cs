namespace Torneio.Domain.Entities;

/// <summary>
/// Prêmio de um torneio — representa o que o X-ésimo colocado recebe.
/// </summary>
public class Premio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }

    /// <summary>Posição premiada (1 = 1º lugar, 2 = 2º lugar, …).</summary>
    public int Posicao { get; private set; }

    /// <summary>Descrição do prêmio, ex: "Troféu + R$ 500,00".</summary>
    public string Descricao { get; private set; } = null!;

    private Premio() { }

    public static Premio Criar(Guid torneioId, int posicao, string descricao)
    {
        if (posicao < 1)
            throw new ArgumentException("A posição deve ser maior que zero.", nameof(posicao));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição do prêmio é obrigatória.", nameof(descricao));

        return new Premio
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Posicao = posicao,
            Descricao = descricao.Trim(),
        };
    }

    public void AtualizarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição do prêmio é obrigatória.", nameof(descricao));
        Descricao = descricao.Trim();
    }
}
