namespace Torneio.Domain.Entities;

public class EspeciePeixe
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? NomeCientifico { get; private set; }
    public string? FotoUrl { get; private set; }

    private EspeciePeixe() { }

    public static EspeciePeixe Criar(string nome, string? nomeCientifico = null, string? fotoUrl = null)
    {
        return new EspeciePeixe
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            NomeCientifico = nomeCientifico,
            FotoUrl = fotoUrl
        };
    }

    public void Atualizar(string nome, string? nomeCientifico, string? fotoUrl = null)
    {
        Nome = nome;
        NomeCientifico = nomeCientifico;
        if (fotoUrl != null) FotoUrl = fotoUrl;
    }
}
