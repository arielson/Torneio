using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.RegistroPublicoMembro;

public class SolicitarRegistroPublicoMembroDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O celular e obrigatorio.")]
    public string Celular { get; init; } = null!;

    public string? TamanhoCamisa { get; init; }
}
