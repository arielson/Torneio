namespace Torneio.Domain.Entities;

public class MensagemTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Titulo { get; private set; } = null!;
    public string Corpo { get; private set; } = null!;
    public string CriadoPor { get; private set; } = null!;
    public DateTime CriadoEm { get; private set; }

    private MensagemTorneio() { }

    public static MensagemTorneio Criar(Guid torneioId, string titulo, string corpo, string criadoPor) => new()
    {
        Id = Guid.NewGuid(),
        TorneioId = torneioId,
        Titulo = titulo,
        Corpo = corpo,
        CriadoPor = criadoPor,
        CriadoEm = DateTime.UtcNow
    };
}
