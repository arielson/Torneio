namespace Torneio.Domain.Entities;

public class LogAuditoria
{
    public Guid Id { get; private set; }
    public Guid? TorneioId { get; private set; }
    public string? NomeTorneio { get; private set; }
    public string Categoria { get; private set; } = null!;
    public string Acao { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public string UsuarioNome { get; private set; } = null!;
    public string UsuarioPerfil { get; private set; } = null!;
    public string? IpAddress { get; private set; }
    public DateTime DataHora { get; private set; }

    private LogAuditoria() { }

    public static LogAuditoria Criar(
        Guid? torneioId,
        string? nomeTorneio,
        string categoria,
        string acao,
        string descricao,
        string usuarioNome,
        string usuarioPerfil,
        string? ipAddress = null)
    {
        return new LogAuditoria
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            NomeTorneio = nomeTorneio,
            Categoria = categoria,
            Acao = acao,
            Descricao = descricao,
            UsuarioNome = usuarioNome,
            UsuarioPerfil = usuarioPerfil,
            IpAddress = ipAddress,
            DataHora = DateTime.UtcNow
        };
    }
}
