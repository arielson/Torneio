using Torneio.Application.DTOs.Equipe;

namespace Torneio.Application.Services.Interfaces;

public interface IEquipeServico
{
    Task<EquipeDto?> ObterPorId(Guid id);
    Task<IEnumerable<EquipeDto>> ListarTodos();
    Task<EquipeDto> Criar(CriarEquipeDto dto);
    Task Atualizar(Guid id, AtualizarEquipeDto dto);
    Task Remover(Guid id);
    Task AdicionarMembro(Guid equipeId, Guid membroId);
    Task RemoverMembro(Guid equipeId, Guid membroId);
}
