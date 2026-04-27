using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.EspeciePeixe;

public class CriarEspeciePeixeDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(200)]
    public string Nome { get; init; } = null!;

    [MaxLength(300)]
    public string? NomeCientifico { get; init; }

    public string? FotoUrl { get; init; }
}
