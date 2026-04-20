using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Fiscal;

public class CriarFiscalDto
{
    public Guid TorneioId { get; init; }

    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O usuario e obrigatorio.")]
    public string Usuario { get; init; } = null!;

    [Required(ErrorMessage = "A senha e obrigatoria.")]
    public string Senha { get; init; } = null!;

    public string? FotoUrl { get; init; }
    public List<Guid> EquipeIds { get; init; } = new();
}
