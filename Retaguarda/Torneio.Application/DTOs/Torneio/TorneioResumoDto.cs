namespace Torneio.Application.DTOs.Torneio;
public class TorneioResumoDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = null!;
    public string NomeTorneio { get; init; } = null!;
    public string? LogoUrl { get; init; }
    public string Status { get; init; } = null!;
    public bool Ativo { get; init; }
    public DateTime CriadoEm { get; init; }
}
