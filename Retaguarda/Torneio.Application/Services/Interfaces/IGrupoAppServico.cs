using Torneio.Application.DTOs.Grupo;

namespace Torneio.Application.Services.Interfaces;

public interface IGrupoAppServico
{
    Task<IEnumerable<GrupoDto>> ListarTodos();
    Task<GrupoDto?> ObterPorId(Guid id);
    Task<GrupoDto> Criar(CriarGrupoDto dto);
    Task Atualizar(Guid id, AtualizarGrupoDto dto);
    Task Remover(Guid id);
    Task AdicionarMembro(Guid grupoId, Guid membroId);
    Task RemoverMembro(Guid grupoMembroId);
}
