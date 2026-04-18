using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Premio;

public class CriarPremioDto
{
    [Range(1, 100, ErrorMessage = "A posição deve ser entre 1 e 100.")]
    public int Posicao { get; init; }

    [Required(ErrorMessage = "A descrição do prêmio é obrigatória.")]
    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    public string Descricao { get; init; } = null!;
}
