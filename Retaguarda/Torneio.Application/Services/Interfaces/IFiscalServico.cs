using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;

namespace Torneio.Application.Services.Interfaces;

public interface IFiscalServico
{
    Task<FiscalDto?> ObterPorId(Guid id);
    Task<IEnumerable<FiscalDto>> ListarTodos();
    Task<FiscalDto> Criar(CriarFiscalDto dto);
    Task Atualizar(Guid id, AtualizarFiscalDto dto);
    Task AtualizarSenha(Guid id, AtualizarSenhaDto dto);
    Task Remover(Guid id);
}
