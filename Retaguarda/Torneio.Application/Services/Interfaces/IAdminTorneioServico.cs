using Torneio.Application.DTOs.AdminTorneio;
using Torneio.Application.DTOs.Auth;

namespace Torneio.Application.Services.Interfaces;

public interface IAdminTorneioServico
{
    Task<AdminTorneioDto?> ObterPorId(Guid id);
    Task<IEnumerable<AdminTorneioDto>> ListarPorTorneio(Guid torneioId);
    Task<AdminTorneioDto> Criar(CriarAdminTorneioDto dto);
    Task AtualizarSenha(Guid id, AtualizarSenhaDto dto);
    Task Remover(Guid id);
}
