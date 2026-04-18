using Torneio.Application.DTOs.Membro;

namespace Torneio.Application.Services.Interfaces;

public interface IMembroServico
{
    Task<MembroDto?> ObterPorId(Guid id);
    Task<IEnumerable<MembroDto>> ListarTodos();
    Task<MembroDto> Criar(CriarMembroDto dto);
    Task Atualizar(Guid id, AtualizarMembroDto dto);
    Task Remover(Guid id);
}
