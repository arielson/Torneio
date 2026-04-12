using Torneio.Application.DTOs.AdminGeral;
using Torneio.Application.DTOs.Auth;

namespace Torneio.Application.Services.Interfaces;

public interface IAdminGeralServico
{
    Task<AdminGeralDto?> ObterPorId(Guid id);
    Task<IEnumerable<AdminGeralDto>> ListarTodos();
    Task<AdminGeralDto> Criar(CriarAdminGeralDto dto);
    Task AtualizarSenha(Guid id, AtualizarSenhaDto dto);
    Task Remover(Guid id);
}
