namespace Torneio.Domain.Entities;

public class ProdutoExtraMembro
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid ProdutoExtraTorneioId { get; private set; }
    public Guid MembroId { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal ValorCobrado { get; private set; }
    public string? Observacao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private ProdutoExtraMembro() { }

    public static ProdutoExtraMembro Criar(
        Guid torneioId,
        Guid produtoExtraTorneioId,
        Guid membroId,
        decimal quantidade,
        decimal valorCobrado,
        string? observacao = null)
    {
        return new ProdutoExtraMembro
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            ProdutoExtraTorneioId = produtoExtraTorneioId,
            MembroId = membroId,
            Quantidade = quantidade,
            ValorCobrado = valorCobrado,
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
    }

    public void Atualizar(decimal quantidade, decimal valorCobrado, string? observacao)
    {
        Quantidade = quantidade;
        ValorCobrado = valorCobrado;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }

    public void Reativar(decimal quantidade, decimal valorCobrado, string? observacao)
    {
        Ativo = true;
        Atualizar(quantidade, valorCobrado, observacao);
    }

    public void Desativar() => Ativo = false;
}
