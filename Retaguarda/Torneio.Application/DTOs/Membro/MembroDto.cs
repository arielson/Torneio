namespace Torneio.Application.DTOs.Membro;

public class MembroDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Nome { get; init; } = null!;
    public string? FotoUrl { get; init; }
    public string? Celular { get; init; }
    public string? TamanhoCamisa { get; init; }
    public string? Usuario { get; init; }
    public bool PossuiSenha { get; init; }
}
