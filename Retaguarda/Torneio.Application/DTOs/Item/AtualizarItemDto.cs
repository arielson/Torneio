using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Item;

public class AtualizarItemDto
{
    public Guid EspeciePeixeId { get; init; }

    public decimal? Comprimento { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O fator multiplicador deve ser maior que zero.")]
    public decimal FatorMultiplicador { get; init; } = 1.0m;
}
