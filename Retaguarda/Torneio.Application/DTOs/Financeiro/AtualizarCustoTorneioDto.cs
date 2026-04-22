using System.ComponentModel.DataAnnotations;
using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Financeiro;

public class AtualizarCustoTorneioDto
{
    public CategoriaCustoTorneio Categoria { get; init; }

    [Required]
    public string Descricao { get; init; } = null!;

    [Range(0.01, 999999999)]
    public decimal Quantidade { get; init; } = 1;

    [Range(0, 999999999)]
    public decimal ValorUnitario { get; init; }
    [DataType(DataType.Date)]
    public DateTime? Vencimento { get; init; }
    public string? Responsavel { get; init; }
    public string? Observacao { get; init; }
}
