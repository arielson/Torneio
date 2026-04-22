namespace Torneio.Domain.Entities;

public class ChecklistTorneioItem
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Item { get; private set; } = null!;
    public DateTime? Data { get; private set; }
    public string? Responsavel { get; private set; }
    public bool Concluido { get; private set; }

    private ChecklistTorneioItem() { }

    public static ChecklistTorneioItem Criar(
        Guid torneioId,
        string item,
        DateTime? data,
        string? responsavel,
        bool concluido)
    {
        return new ChecklistTorneioItem
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Item = item,
            Data = data,
            Responsavel = string.IsNullOrWhiteSpace(responsavel) ? null : responsavel.Trim(),
            Concluido = concluido
        };
    }

    public void Atualizar(string item, DateTime? data, string? responsavel, bool concluido)
    {
        Item = item;
        Data = data;
        Responsavel = string.IsNullOrWhiteSpace(responsavel) ? null : responsavel.Trim();
        Concluido = concluido;
    }
}
