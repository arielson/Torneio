using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class ProdutoExtraTorneioServico : IProdutoExtraTorneioServico
{
    private readonly IProdutoExtraTorneioRepositorio _produtoRepositorio;
    private readonly IProdutoExtraMembroRepositorio _produtoMembroRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IParcelaTorneioRepositorio _parcelaRepositorio;
    private readonly ITenantContext _tenantContext;
    private readonly IFinanceiroTorneioServico _financeiroServico;

    public ProdutoExtraTorneioServico(
        IProdutoExtraTorneioRepositorio produtoRepositorio,
        IProdutoExtraMembroRepositorio produtoMembroRepositorio,
        IMembroRepositorio membroRepositorio,
        IParcelaTorneioRepositorio parcelaRepositorio,
        ITenantContext tenantContext,
        IFinanceiroTorneioServico financeiroServico)
    {
        _produtoRepositorio = produtoRepositorio;
        _produtoMembroRepositorio = produtoMembroRepositorio;
        _membroRepositorio = membroRepositorio;
        _parcelaRepositorio = parcelaRepositorio;
        _tenantContext = tenantContext;
        _financeiroServico = financeiroServico;
    }

    public async Task<IEnumerable<ProdutoExtraTorneioDto>> ListarProdutos(Guid torneioId)
    {
        var produtos = (await _produtoRepositorio.ListarPorTorneio(torneioId)).ToList();
        var adesoes = (await _produtoMembroRepositorio.ListarPorTorneio(torneioId)).Where(x => x.Ativo).ToList();

        return produtos.Select(x => new ProdutoExtraTorneioDto
        {
            Id = x.Id,
            TorneioId = x.TorneioId,
            Nome = x.Nome,
            Descricao = x.Descricao,
            Valor = x.Valor,
            Ativo = x.Ativo,
            QuantidadeAderidos = adesoes.Count(a => a.ProdutoExtraTorneioId == x.Id)
        });
    }

    public async Task<ProdutoExtraTorneioDto?> ObterProduto(Guid id)
    {
        var produto = await _produtoRepositorio.ObterPorId(id);
        if (produto is null || produto.TorneioId != _tenantContext.TorneioId)
            return null;

        var aderidos = (await _produtoMembroRepositorio.ListarPorProduto(id)).Count(x => x.Ativo);
        return new ProdutoExtraTorneioDto
        {
            Id = produto.Id,
            TorneioId = produto.TorneioId,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            Valor = produto.Valor,
            Ativo = produto.Ativo,
            QuantidadeAderidos = aderidos
        };
    }

    public async Task<ProdutoExtraTorneioDto> CriarProduto(CriarProdutoExtraTorneioDto dto)
    {
        var produto = ProdutoExtraTorneio.Criar(dto.TorneioId, dto.Nome, dto.Valor, dto.Descricao);
        await _produtoRepositorio.Adicionar(produto);
        return new ProdutoExtraTorneioDto
        {
            Id = produto.Id,
            TorneioId = produto.TorneioId,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            Valor = produto.Valor,
            Ativo = produto.Ativo,
            QuantidadeAderidos = 0
        };
    }

    public async Task AtualizarProduto(Guid id, AtualizarProdutoExtraTorneioDto dto)
    {
        var produto = await _produtoRepositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Produto extra '{id}' nao encontrado.");
        if (produto.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Produto extra '{id}' nao encontrado.");

        produto.Atualizar(dto.Nome, dto.Valor, dto.Descricao);
        if (dto.Ativo) produto.Ativar(); else produto.Desativar();
        await _produtoRepositorio.Atualizar(produto);

        var adesoes = (await _produtoMembroRepositorio.ListarPorProduto(id)).Where(x => x.Ativo).ToList();
        foreach (var adesao in adesoes)
        {
            var parcela = (await _parcelaRepositorio.ListarPorMembro(adesao.MembroId))
                .FirstOrDefault(x => x.TipoParcela == TipoParcelaTorneio.ProdutoExtra && x.ReferenciaId == adesao.Id);
            if (parcela?.Pago == true)
                continue;

            adesao.Atualizar(adesao.Quantidade, dto.Valor * adesao.Quantidade, adesao.Observacao);
            await _produtoMembroRepositorio.Atualizar(adesao);
        }

        await _financeiroServico.SincronizarParcelas(_tenantContext.TorneioId);
    }

    public async Task RemoverProduto(Guid id)
    {
        var produto = await _produtoRepositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Produto extra '{id}' nao encontrado.");
        if (produto.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Produto extra '{id}' nao encontrado.");

        var adesoes = (await _produtoMembroRepositorio.ListarPorProduto(id)).Where(x => x.Ativo).ToList();
        foreach (var adesao in adesoes)
        {
            var parcela = (await _parcelaRepositorio.ListarPorMembro(adesao.MembroId))
                .FirstOrDefault(x => x.TipoParcela == TipoParcelaTorneio.ProdutoExtra && x.ReferenciaId == adesao.Id);
            if (parcela?.Pago == true)
                throw new InvalidOperationException("Nao e possivel remover o produto extra porque ja existem cobrancas pagas vinculadas a ele.");
        }

        foreach (var adesao in adesoes)
        {
            adesao.Desativar();
            await _produtoMembroRepositorio.Atualizar(adesao);
        }

        await _produtoRepositorio.Remover(id);
        await _financeiroServico.SincronizarParcelas(_tenantContext.TorneioId);
    }

    public async Task<IEnumerable<ProdutoExtraMembroDto>> ListarAderidos(Guid produtoExtraTorneioId)
    {
        var membros = (await _membroRepositorio.ListarPorTorneio(_tenantContext.TorneioId)).ToDictionary(x => x.Id, x => x.Nome);
        var parcelas = (await _parcelaRepositorio.ListarPorTorneio(_tenantContext.TorneioId))
            .Where(x => x.TipoParcela == TipoParcelaTorneio.ProdutoExtra && x.ReferenciaId.HasValue)
            .ToDictionary(x => x.ReferenciaId!.Value);

        return (await _produtoMembroRepositorio.ListarPorProduto(produtoExtraTorneioId))
            .Where(x => x.Ativo)
            .Select(x =>
            {
                parcelas.TryGetValue(x.Id, out var parcela);
                return new ProdutoExtraMembroDto
                {
                    Id = x.Id,
                    TorneioId = x.TorneioId,
                    ProdutoExtraTorneioId = x.ProdutoExtraTorneioId,
                    MembroId = x.MembroId,
                    NomeMembro = membros.TryGetValue(x.MembroId, out var nome) ? nome : string.Empty,
                    Quantidade = x.Quantidade,
                    ValorCobrado = x.ValorCobrado,
                    Observacao = x.Observacao,
                    Ativo = x.Ativo,
                    ParcelaId = parcela?.Id,
                    Pago = parcela?.Pago == true,
                    DataPagamento = parcela?.DataPagamento,
                    Inadimplente = parcela is not null && !parcela.Pago && parcela.Vencimento.Date < DateTime.UtcNow.Date
                };
            })
            .OrderBy(x => x.NomeMembro)
            .ToList();
    }

    public async Task AdicionarMembro(CriarProdutoExtraMembroDto dto)
    {
        var produto = await _produtoRepositorio.ObterPorId(dto.ProdutoExtraTorneioId)
            ?? throw new KeyNotFoundException($"Produto extra '{dto.ProdutoExtraTorneioId}' nao encontrado.");
        if (produto.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Produto extra '{dto.ProdutoExtraTorneioId}' nao encontrado.");

        var membro = await _membroRepositorio.ObterPorId(dto.MembroId)
            ?? throw new KeyNotFoundException($"Membro '{dto.MembroId}' nao encontrado.");
        if (membro.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Membro '{dto.MembroId}' nao encontrado.");

        if (dto.Quantidade <= 0)
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");

        var valorCalculado = produto.Valor * dto.Quantidade;

        var existente = await _produtoMembroRepositorio.ObterPorProdutoEMembro(dto.ProdutoExtraTorneioId, dto.MembroId);
        if (existente is null)
        {
            existente = ProdutoExtraMembro.Criar(
                dto.TorneioId,
                dto.ProdutoExtraTorneioId,
                dto.MembroId,
                dto.Quantidade,
                valorCalculado,
                dto.Observacao);
            await _produtoMembroRepositorio.Adicionar(existente);
        }
        else
        {
            existente.Reativar(dto.Quantidade, valorCalculado, dto.Observacao);
            await _produtoMembroRepositorio.Atualizar(existente);
        }

        await _financeiroServico.SincronizarParcelas(dto.TorneioId);
    }

    public async Task RemoverMembro(Guid produtoExtraMembroId)
    {
        var adesao = await _produtoMembroRepositorio.ObterPorId(produtoExtraMembroId)
            ?? throw new KeyNotFoundException($"Adesao '{produtoExtraMembroId}' nao encontrada.");
        if (adesao.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Adesao '{produtoExtraMembroId}' nao encontrada.");

        var parcela = (await _parcelaRepositorio.ListarPorMembro(adesao.MembroId))
            .FirstOrDefault(x => x.TipoParcela == TipoParcelaTorneio.ProdutoExtra && x.ReferenciaId == adesao.Id);
        if (parcela?.Pago == true)
            throw new InvalidOperationException("Nao e possivel remover a adesao porque a cobranca do produto extra ja foi paga.");

        adesao.Desativar();
        await _produtoMembroRepositorio.Atualizar(adesao);
        await _financeiroServico.SincronizarParcelas(adesao.TorneioId);
    }
}
