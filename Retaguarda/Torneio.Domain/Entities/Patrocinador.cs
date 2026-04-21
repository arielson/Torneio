namespace Torneio.Domain.Entities;

public class Patrocinador
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string FotoUrl { get; private set; } = null!;
    public string? Instagram { get; private set; }
    public string? Site { get; private set; }
    public string? Zap { get; private set; }

    private Patrocinador() { }

    public static Patrocinador Criar(
        Guid torneioId,
        string nome,
        string fotoUrl,
        string? instagram,
        string? site,
        string? zap)
    {
        return new Patrocinador
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome,
            FotoUrl = fotoUrl,
            Instagram = instagram,
            Site = site,
            Zap = zap
        };
    }

    public void Atualizar(
        string nome,
        string? fotoUrl,
        string? instagram,
        string? site,
        string? zap)
    {
        Nome = nome;
        if (!string.IsNullOrWhiteSpace(fotoUrl))
        {
            FotoUrl = fotoUrl;
        }

        Instagram = instagram;
        Site = site;
        Zap = zap;
    }
}
