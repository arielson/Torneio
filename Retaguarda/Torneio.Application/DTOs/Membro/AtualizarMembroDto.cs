using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Membro;

public class AtualizarMembroDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    public string? FotoUrl { get; init; }
}
