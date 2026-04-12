using Torneio.Application.DTOs.Torneio;

namespace Torneio.Application.Services.Interfaces;

public interface ITorneioServico
{
    Task<TorneioDto?> ObterPorId(Guid id);
    Task<TorneioDto?> ObterPorSlug(string slug);
    Task<IEnumerable<TorneioDto>> ListarTodos();
    Task<IEnumerable<TorneioDto>> ListarAtivos();
    Task<TorneioDto> Criar(CriarTorneioDto dto);
    Task Atualizar(Guid id, AtualizarTorneioDto dto);
    Task Ativar(Guid id);
    Task Desativar(Guid id);
}
