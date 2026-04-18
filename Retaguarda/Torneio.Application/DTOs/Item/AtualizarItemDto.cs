using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Item;

public class AtualizarItemDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    public decimal Comprimento { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O fator multiplicador deve ser maior que zero.")]
    public decimal FatorMultiplicador { get; init; }

    public string? FotoUrl { get; init; }
}
