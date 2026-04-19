namespace Torneio.Application.DTOs.Log;

public class RegistrarLogDto
{
    public Guid? TorneioId { get; init; }
    public string? NomeTorneio { get; init; }
    public string Categoria { get; init; } = null!;
    public string Acao { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public string UsuarioNome { get; init; } = null!;
    public string UsuarioPerfil { get; init; } = null!;
    public string? IpAddress { get; init; }
}

/// <summary>Categorias padronizadas de log.</summary>
public static class CategoriaLog
{
    public const string Usuarios  = "Usuários";
    public const string Capturas  = "Capturas";
    public const string Itens     = "Itens";
    public const string Membros   = "Membros";
    public const string Premios   = "Prêmios";
    public const string Torneios  = "Torneios";
    public const string Sorteio   = "Sorteio";

    public static readonly IReadOnlyList<string> Todas =
        [Usuarios, Capturas, Itens, Membros, Premios, Torneios, Sorteio];
}
