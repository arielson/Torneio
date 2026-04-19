namespace Torneio.Application.DTOs.Log;

public class LogAuditoriaDto
{
    public Guid Id { get; init; }
    public Guid? TorneioId { get; init; }
    public string? NomeTorneio { get; init; }
    public string Categoria { get; init; } = null!;
    public string Acao { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public string UsuarioNome { get; init; } = null!;
    public string UsuarioPerfil { get; init; } = null!;
    public string? IpAddress { get; init; }
    public DateTime DataHora { get; init; }
}
