namespace Torneio.Domain.Entities;

public class Membro
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? FotoUrl { get; private set; }
    public string? Celular { get; private set; }
    public string? TamanhoCamisa { get; private set; }
    public string? Usuario { get; private set; }
    public string? SenhaHash { get; private set; }

    private Membro() { }

    public static Membro Criar(
        Guid torneioId,
        string nome,
        string? fotoUrl = null,
        string? celular = null,
        string? tamanhoCamisa = null,
        string? usuario = null,
        string? senhaHash = null)
    {
        return new Membro
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome,
            FotoUrl = fotoUrl,
            Celular = string.IsNullOrWhiteSpace(celular) ? null : celular.Trim(),
            TamanhoCamisa = string.IsNullOrWhiteSpace(tamanhoCamisa) ? null : tamanhoCamisa.Trim(),
            Usuario = string.IsNullOrWhiteSpace(usuario) ? null : usuario.Trim(),
            SenhaHash = string.IsNullOrWhiteSpace(senhaHash) ? null : senhaHash.Trim()
        };
    }

    public void AtualizarNome(string nome) => Nome = nome;

    public void AtualizarFoto(string fotoUrl) => FotoUrl = fotoUrl;

    public void AtualizarCelular(string? celular) =>
        Celular = string.IsNullOrWhiteSpace(celular) ? null : celular.Trim();

    public void AtualizarTamanhoCamisa(string? tamanhoCamisa) =>
        TamanhoCamisa = string.IsNullOrWhiteSpace(tamanhoCamisa) ? null : tamanhoCamisa.Trim();

    public void AtualizarCredenciais(string? usuario, string? senhaHash)
    {
        if (!string.IsNullOrWhiteSpace(usuario))
            Usuario = usuario.Trim();

        if (!string.IsNullOrWhiteSpace(senhaHash))
            SenhaHash = senhaHash.Trim();
    }
}
