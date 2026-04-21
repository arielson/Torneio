using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Patrocinador;

public class CriarPatrocinadorDto
{
    public Guid TorneioId { get; init; }

    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "A imagem e obrigatoria.")]
    public string FotoUrl { get; init; } = null!;

    public string? Instagram { get; init; }
    public string? Site { get; init; }
    public string? Zap { get; init; }
    public bool ExibirNaTelaInicial { get; init; } = true;
    public bool ExibirNosRelatorios { get; init; } = true;
}
