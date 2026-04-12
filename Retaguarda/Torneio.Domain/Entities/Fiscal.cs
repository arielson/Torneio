namespace Torneio.Domain.Entities;

public class Fiscal
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public Guid AnoTorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? FotoUrl { get; private set; }
    public string Usuario { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;

    private Fiscal() { }

    public static Fiscal Criar(
        Guid torneioId,
        Guid anoTorneioId,
        string nome,
        string usuario,
        string senhaHash,
        string? fotoUrl = null)
    {
        return new Fiscal
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            AnoTorneioId = anoTorneioId,
            Nome = nome,
            Usuario = usuario,
            SenhaHash = senhaHash,
            FotoUrl = fotoUrl
        };
    }

    public void AtualizarSenha(string novaSenhaHash) => SenhaHash = novaSenhaHash;

    public void AtualizarFoto(string fotoUrl) => FotoUrl = fotoUrl;

    public void AtualizarNome(string nome) => Nome = nome;
}
