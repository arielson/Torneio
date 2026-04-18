namespace Torneio.Domain.Entities;

public class Membro
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? FotoUrl { get; private set; }

    private Membro() { }

    public static Membro Criar(
        Guid torneioId,
        string nome,
        string? fotoUrl = null)
    {
        return new Membro
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome,
            FotoUrl = fotoUrl
        };
    }

    public void AtualizarNome(string nome) => Nome = nome;

    public void AtualizarFoto(string fotoUrl) => FotoUrl = fotoUrl;
}
