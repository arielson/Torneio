namespace Torneio.Domain.Entities;

public class ProdutoExtraTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private ProdutoExtraTorneio() { }

    public static ProdutoExtraTorneio Criar(Guid torneioId, string nome, decimal valor, string? descricao = null)
    {
        return new ProdutoExtraTorneio
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            Valor = valor,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
    }

    public void Atualizar(string nome, decimal valor, string? descricao)
    {
        Nome = nome.Trim();
        Valor = valor;
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
    }

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;
}
