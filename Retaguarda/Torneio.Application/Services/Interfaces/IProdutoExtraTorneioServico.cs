using Torneio.Application.DTOs.Financeiro;

namespace Torneio.Application.Services.Interfaces;

public interface IProdutoExtraTorneioServico
{
    Task<IEnumerable<ProdutoExtraTorneioDto>> ListarProdutos(Guid torneioId);
    Task<ProdutoExtraTorneioDto?> ObterProduto(Guid id);
    Task<ProdutoExtraTorneioDto> CriarProduto(CriarProdutoExtraTorneioDto dto);
    Task AtualizarProduto(Guid id, AtualizarProdutoExtraTorneioDto dto);
    Task RemoverProduto(Guid id);
    Task<IEnumerable<ProdutoExtraMembroDto>> ListarAderidos(Guid produtoExtraTorneioId);
    Task AdicionarMembro(CriarProdutoExtraMembroDto dto);
    Task RemoverMembro(Guid produtoExtraMembroId);
}
