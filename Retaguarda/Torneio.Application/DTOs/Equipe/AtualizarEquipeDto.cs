using Torneio.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Torneio.Application.DTOs.Equipe;

public class AtualizarEquipeDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O capitão é obrigatório.")]
    public string Capitao { get; init; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Informe ao menos 1 vaga.")]
    public int QtdVagas { get; init; }
    public decimal Custo { get; init; }
    public StatusEmbarcacaoFinanceira StatusFinanceiro { get; init; } = StatusEmbarcacaoFinanceira.Pendente;
    public DateTime? DataVencimentoCusto { get; init; }

    public string? FotoUrl { get; init; }
    public string? FotoCapitaoUrl { get; init; }
}
