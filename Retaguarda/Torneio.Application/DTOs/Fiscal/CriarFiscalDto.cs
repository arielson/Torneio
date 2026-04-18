using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Fiscal;

public class CriarFiscalDto
{
    public Guid TorneioId { get; init; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O usuário é obrigatório.")]
    public string Usuario { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; init; } = null!;

    public string? FotoUrl { get; init; }
}
