using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Membro;

public class AtualizarMembroDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    public string? FotoUrl { get; init; }
    public string? Celular { get; init; }
    public string? TamanhoCamisa { get; init; }
}
