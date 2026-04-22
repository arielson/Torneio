using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class CustoTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public CategoriaCustoTorneio Categoria { get; private set; }
    public string Descricao { get; private set; } = null!;
    public decimal Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime? Vencimento { get; private set; }
    public string? Responsavel { get; private set; }
    public string? Observacao { get; private set; }

    private CustoTorneio() { }

    public static CustoTorneio Criar(
        Guid torneioId,
        CategoriaCustoTorneio categoria,
        string descricao,
        decimal quantidade,
        decimal valorUnitario,
        DateTime? vencimento,
        string? responsavel,
        string? observacao)
    {
        return new CustoTorneio
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Categoria = categoria,
            Descricao = descricao,
            Quantidade = quantidade,
            ValorUnitario = valorUnitario,
            ValorTotal = quantidade * valorUnitario,
            Vencimento = vencimento?.Date,
            Responsavel = string.IsNullOrWhiteSpace(responsavel) ? null : responsavel.Trim(),
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim()
        };
    }

    public void Atualizar(
        CategoriaCustoTorneio categoria,
        string descricao,
        decimal quantidade,
        decimal valorUnitario,
        DateTime? vencimento,
        string? responsavel,
        string? observacao)
    {
        Categoria = categoria;
        Descricao = descricao;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorTotal = quantidade * valorUnitario;
        Vencimento = vencimento?.Date;
        Responsavel = string.IsNullOrWhiteSpace(responsavel) ? null : responsavel.Trim();
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }
}
