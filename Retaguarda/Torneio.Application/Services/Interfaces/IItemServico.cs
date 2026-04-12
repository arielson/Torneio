using Torneio.Application.DTOs.Item;

namespace Torneio.Application.Services.Interfaces;

public interface IItemServico
{
    Task<ItemDto?> ObterPorId(Guid id);
    Task<IEnumerable<ItemDto>> ListarPorTorneio(Guid torneioId);
    Task<ItemDto> Criar(CriarItemDto dto);
    Task Atualizar(Guid id, AtualizarItemDto dto);
    Task Remover(Guid id);
}
