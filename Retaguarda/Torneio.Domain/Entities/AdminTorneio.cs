namespace Torneio.Domain.Entities;

public class AdminTorneio
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Usuario { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public bool DeveAlterarSenha { get; private set; }
    public DateTime? UltimoAcessoEm { get; private set; }
    public DateTime? SenhaAlteradaEm { get; private set; }

    private AdminTorneio() { }

    public static AdminTorneio Criar(
        Guid usuarioId,
        Guid torneioId,
        string nome,
        string usuario,
        string senhaHash)
    {
        return new AdminTorneio
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TorneioId = torneioId,
            Nome = nome,
            Usuario = usuario,
            SenhaHash = senhaHash,
            DeveAlterarSenha = true
        };
    }

    public void AtualizarSenha(string novaSenhaHash)
    {
        SenhaHash = novaSenhaHash;
        DeveAlterarSenha = false;
        SenhaAlteradaEm = DateTime.UtcNow;
    }

    public void RedefinirSenha(string novaSenhaHash)
    {
        SenhaHash = novaSenhaHash;
        DeveAlterarSenha = true;
        SenhaAlteradaEm = DateTime.UtcNow;
    }

    public void RegistrarAcesso() => UltimoAcessoEm = DateTime.UtcNow;

    public void AtualizarNome(string nome) => Nome = nome;
}
