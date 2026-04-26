using Torneio.Application.DTOs.Auth;

namespace Torneio.Application.Services.Interfaces;

public interface IAutenticacaoServico
{
    Task<UsuarioAutenticadoDto?> AutenticarAdminGeral(string usuario, string senha);
    Task<UsuarioAutenticadoDto?> AutenticarAdminTorneio(string usuario, string senha, Guid torneioId);
    Task<UsuarioAutenticadoDto?> AutenticarFiscal(string usuario, string senha, Guid torneioId);
    Task<UsuarioAutenticadoDto?> AutenticarMembro(string usuario, string senha, Guid torneioId);
    /// <summary>Troca a senha e limpa DeveAlterarSenha. Lança exceção se senhaAtual for inválida.</summary>
    Task TrocarSenha(Guid usuarioId, string perfil, string senhaAtual, string novaSenha, Guid? torneioId);
}
