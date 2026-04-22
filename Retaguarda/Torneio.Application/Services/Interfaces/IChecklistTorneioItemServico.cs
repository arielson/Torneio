using Torneio.Application.DTOs.Financeiro;

namespace Torneio.Application.Services.Interfaces;

public interface IChecklistTorneioItemServico
{
    Task<IEnumerable<ChecklistTorneioItemDto>> Listar(Guid torneioId);
    Task<ChecklistTorneioItemDto?> ObterPorId(Guid id);
    Task<ChecklistTorneioItemDto> Criar(CriarChecklistTorneioItemDto dto);
    Task Atualizar(Guid id, AtualizarChecklistTorneioItemDto dto);
    Task Remover(Guid id);
}
