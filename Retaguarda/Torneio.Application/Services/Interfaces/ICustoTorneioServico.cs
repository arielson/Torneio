using Torneio.Application.DTOs.Financeiro;

namespace Torneio.Application.Services.Interfaces;

public interface ICustoTorneioServico
{
    Task<IEnumerable<CustoTorneioDto>> Listar(Guid torneioId);
    Task<CustoTorneioDto?> ObterPorId(Guid id);
    Task<CustoTorneioDto> Criar(CriarCustoTorneioDto dto);
    Task Atualizar(Guid id, AtualizarCustoTorneioDto dto);
    Task Remover(Guid id);
}
