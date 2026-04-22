namespace Torneio.Application.DTOs.Financeiro;

public class ChecklistTorneioItemDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Item { get; init; } = null!;
    public DateTime? Data { get; init; }
    public string? Responsavel { get; init; }
    public bool Concluido { get; init; }
}
