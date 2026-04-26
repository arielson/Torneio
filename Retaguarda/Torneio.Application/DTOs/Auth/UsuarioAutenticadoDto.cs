using Torneio.Domain.Enums;

namespace Torneio.Application.DTOs.Auth;

public class UsuarioAutenticadoDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Usuario { get; init; } = null!;
    public PerfilUsuario Perfil { get; init; }
    public Guid? TorneioId { get; init; }
    public string? Slug { get; init; }
    public bool DeveAlterarSenha { get; init; }
}
