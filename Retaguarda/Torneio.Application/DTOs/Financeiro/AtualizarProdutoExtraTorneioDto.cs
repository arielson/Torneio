using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Financeiro;

public class AtualizarProdutoExtraTorneioDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Range(0, 999999999, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
    public decimal Valor { get; init; }

    public string? Descricao { get; init; }
    public bool Ativo { get; init; } = true;
}
