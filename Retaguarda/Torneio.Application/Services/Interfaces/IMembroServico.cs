using Torneio.Application.DTOs.Membro;

namespace Torneio.Application.Services.Interfaces;

public interface IMembroServico
{
    Task<MembroDto?> ObterPorId(Guid id);
    Task<IEnumerable<MembroDto>> ListarTodos();
    Task<MembroDto> Criar(CriarMembroDto dto);
    Task Atualizar(Guid id, AtualizarMembroDto dto);
    Task Remover(Guid id);
    Task<RecuperacaoSenhaMembroSolicitadaDto> SolicitarRecuperacaoSenha(Guid torneioId, string nomeTorneio, SolicitarRecuperacaoSenhaMembroDto dto, string? ipAddress);
    Task RedefinirSenha(Guid torneioId, string nomeTorneio, ConfirmarRecuperacaoSenhaMembroDto dto, string? ipAddress);
}
