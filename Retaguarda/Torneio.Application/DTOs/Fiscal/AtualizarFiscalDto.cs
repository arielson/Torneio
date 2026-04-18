using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Fiscal;

public class AtualizarFiscalDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O usuário é obrigatório.")]
    public string Usuario { get; init; } = null!;

    public string? Senha { get; init; }

    public string? FotoUrl { get; init; }
}
