namespace Torneio.Domain.Entities;

public class Membro
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? FotoUrl { get; private set; }
    public string? TamanhoCamisa { get; private set; }

    private Membro() { }

    public static Membro Criar(
        Guid torneioId,
        string nome,
        string? fotoUrl = null,
        string? tamanhoCamisa = null)
    {
        return new Membro
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome,
            FotoUrl = fotoUrl,
            TamanhoCamisa = string.IsNullOrWhiteSpace(tamanhoCamisa) ? null : tamanhoCamisa.Trim()
        };
    }

    public void AtualizarNome(string nome) => Nome = nome;

    public void AtualizarFoto(string fotoUrl) => FotoUrl = fotoUrl;

    public void AtualizarTamanhoCamisa(string? tamanhoCamisa) =>
        TamanhoCamisa = string.IsNullOrWhiteSpace(tamanhoCamisa) ? null : tamanhoCamisa.Trim();
}
