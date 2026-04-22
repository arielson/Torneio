using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Financeiro;

public class CriarChecklistTorneioItemDto
{
    public Guid TorneioId { get; init; }

    [Required]
    public string Item { get; init; } = null!;

    public DateTime? Data { get; init; }
    public string? Responsavel { get; init; }
    public bool Concluido { get; init; }
}
