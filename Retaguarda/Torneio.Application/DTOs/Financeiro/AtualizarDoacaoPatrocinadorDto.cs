using System.ComponentModel.DataAnnotations;
using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Financeiro;

public class AtualizarDoacaoPatrocinadorDto
{
    public Guid? PatrocinadorId { get; init; }

    public string NomePatrocinador { get; init; } = string.Empty;

    [Required(ErrorMessage = "Informe o tipo da doacao.")]
    public TipoDoacaoPatrocinador Tipo { get; init; }

    [Required(ErrorMessage = "Informe a descricao da doacao.")]
    public string Descricao { get; init; } = string.Empty;

    [Range(0, 999999999, ErrorMessage = "A quantidade deve ser maior ou igual a zero.")]
    public decimal? Quantidade { get; init; }

    [Range(0, 999999999, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
    public decimal? Valor { get; init; }

    public string? Observacao { get; init; }
    public DateTime DataDoacao { get; init; } = DateTime.UtcNow.Date;
}
