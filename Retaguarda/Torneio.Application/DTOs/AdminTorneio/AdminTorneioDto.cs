namespace Torneio.Application.DTOs.AdminTorneio;

public class AdminTorneioDto
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public Guid TorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string Usuario { get; init; } = null!;
}
