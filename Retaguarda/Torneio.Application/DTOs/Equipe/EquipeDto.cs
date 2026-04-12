namespace Torneio.Application.DTOs.Equipe;

public class EquipeDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public Guid AnoTorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string? FotoUrl { get; init; }
    public string Capitao { get; init; } = null!;
    public string? FotoCapitaoUrl { get; init; }
    public Guid FiscalId { get; init; }
    public int QtdVagas { get; init; }
    public int QtdMembros { get; init; }
}
