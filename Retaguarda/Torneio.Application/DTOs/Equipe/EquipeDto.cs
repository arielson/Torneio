using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Equipe;

public class EquipeDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string? FotoUrl { get; init; }
    public string Capitao { get; init; } = null!;
    public string? FotoCapitaoUrl { get; init; }
    public int QtdVagas { get; init; }
    public decimal Custo { get; init; }
    public StatusEmbarcacaoFinanceira StatusFinanceiro { get; init; }
    public int QtdMembros { get; init; }
    public List<Guid> MembroIds { get; init; } = new();
    public List<Guid> FiscalIds { get; init; } = new();
}
