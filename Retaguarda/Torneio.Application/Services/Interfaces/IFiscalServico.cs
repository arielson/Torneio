using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;

namespace Torneio.Application.Services.Interfaces;

public interface IFiscalServico
{
    Task<FiscalDto?> ObterPorId(Guid id);
    Task<IEnumerable<FiscalDto>> ListarPorAnoTorneio(Guid anoTorneioId);
    Task<FiscalDto> Criar(CriarFiscalDto dto);
    Task AtualizarSenha(Guid id, AtualizarSenhaDto dto);
    Task Remover(Guid id);
}
