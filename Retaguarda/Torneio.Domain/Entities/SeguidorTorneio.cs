namespace Torneio.Domain.Entities;

public class SeguidorTorneio
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string DeviceToken { get; private set; } = null!;
    public string Plataforma { get; private set; } = null!;
    public DateTime CriadoEm { get; private set; }

    private SeguidorTorneio() { }

    public static SeguidorTorneio Criar(Guid torneioId, string deviceToken, string plataforma) => new()
    {
        Id = Guid.NewGuid(),
        TorneioId = torneioId,
        DeviceToken = deviceToken,
        Plataforma = plataforma,
        CriadoEm = DateTime.UtcNow
    };
}
