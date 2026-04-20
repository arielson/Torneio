using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Fiscal;

public class AtualizarFiscalDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O usuario e obrigatorio.")]
    public string Usuario { get; init; } = null!;

    public string? Senha { get; init; }
    public string? FotoUrl { get; init; }
    public List<Guid> EquipeIds { get; init; } = new();
}
