namespace Torneio.Domain.Entities;

public class Patrocinador
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string FotoUrl { get; private set; } = null!;
    public string? Instagram { get; private set; }
    public string? Facebook { get; private set; }
    public string? Site { get; private set; }
    public string? Zap { get; private set; }
    public bool ExibirNaTelaInicial { get; private set; }
    public bool ExibirNosRelatorios { get; private set; }

    private Patrocinador() { }

    public static Patrocinador Criar(
        Guid torneioId,
        string nome,
        string fotoUrl,
        string? instagram,
        string? facebook,
        string? site,
        string? zap,
        bool exibirNaTelaInicial,
        bool exibirNosRelatorios)
    {
        return new Patrocinador
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome,
            FotoUrl = fotoUrl,
            Instagram = instagram,
            Facebook = facebook,
            Site = site,
            Zap = zap,
            ExibirNaTelaInicial = exibirNaTelaInicial,
            ExibirNosRelatorios = exibirNosRelatorios
        };
    }

    public void Atualizar(
        string nome,
        string? fotoUrl,
        string? instagram,
        string? facebook,
        string? site,
        string? zap,
        bool exibirNaTelaInicial,
        bool exibirNosRelatorios)
    {
        Nome = nome;
        if (!string.IsNullOrWhiteSpace(fotoUrl))
        {
            FotoUrl = fotoUrl;
        }

        Instagram = instagram;
        Facebook = facebook;
        Site = site;
        Zap = zap;
        ExibirNaTelaInicial = exibirNaTelaInicial;
        ExibirNosRelatorios = exibirNosRelatorios;
    }
}
