namespace Torneio.Application.DTOs.Notificacao;

public class MensagemTorneioDto
{
    public Guid Id { get; init; }
    public Guid TorneioId { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Corpo { get; init; } = string.Empty;
    public string CriadoPor { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; }
}
