using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Financeiro;

public class AtualizarParcelaTorneioDto
{
    [Required]
    public DateTime Vencimento { get; init; }
    public string? Observacao { get; init; }
}
