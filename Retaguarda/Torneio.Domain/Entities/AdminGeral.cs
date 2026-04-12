namespace Torneio.Domain.Entities;

public class AdminGeral
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Usuario { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;

    private AdminGeral() { }

    public static AdminGeral Criar(string nome, string usuario, string senhaHash)
    {
        return new AdminGeral
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Usuario = usuario,
            SenhaHash = senhaHash
        };
    }

    public void AtualizarSenha(string novaSenhaHash) => SenhaHash = novaSenhaHash;

    public void AtualizarNome(string nome) => Nome = nome;
}
