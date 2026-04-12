using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Item;

public class CriarItemDto
{
    public Guid TorneioId { get; init; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    [Range(0.01, double.MaxValue, ErrorMessage = "Informe um comprimento maior que zero.")]
    public decimal Comprimento { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O fator multiplicador deve ser maior que zero.")]
    public decimal FatorMultiplicador { get; init; } = 1.0m;

    public string? FotoUrl { get; init; }
}
