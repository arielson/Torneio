using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Patrocinador;

public class AtualizarPatrocinadorDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    public string? FotoUrl { get; init; }
    public string? Instagram { get; init; }
    public string? Facebook { get; init; }
    public string? Site { get; init; }
    public string? Zap { get; init; }
    public bool ExibirNaTelaInicial { get; init; } = true;
    public bool ExibirNosRelatorios { get; init; } = true;
}
