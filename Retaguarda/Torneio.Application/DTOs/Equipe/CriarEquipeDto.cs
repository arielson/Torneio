using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Equipe;

public class CriarEquipeDto
{
    public Guid TorneioId { get; init; }
    public Guid AnoTorneioId { get; init; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O capitão é obrigatório.")]
    public string Capitao { get; init; } = null!;

    public Guid FiscalId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe ao menos 1 vaga.")]
    public int QtdVagas { get; init; }

    public string? FotoUrl { get; init; }
    public string? FotoCapitaoUrl { get; init; }
}
