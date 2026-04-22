using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class CustoTorneioServico : ICustoTorneioServico
{
    private readonly ICustoTorneioRepositorio _repositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly ITenantContext _tenantContext;

    public CustoTorneioServico(
        ICustoTorneioRepositorio repositorio,
        IEquipeRepositorio equipeRepositorio,
        ITenantContext tenantContext)
    {
        _repositorio = repositorio;
        _equipeRepositorio = equipeRepositorio;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<CustoTorneioDto>> Listar(Guid torneioId)
    {
        var custos = (await _repositorio.ListarPorTorneio(torneioId)).Select(ParaDto).ToList();
        var equipes = await _equipeRepositorio.ListarPorTorneio(torneioId);

        custos.AddRange(equipes
            .Where(x => x.Custo > 0)
            .Select(x => new CustoTorneioDto
            {
                Id = Guid.Empty,
                TorneioId = torneioId,
                Categoria = "Embarcacao",
                Descricao = x.Nome,
                Quantidade = 1,
                ValorUnitario = x.Custo,
                ValorTotal = x.Custo,
                Responsavel = x.Capitao,
                Observacao = $"Status: {x.StatusFinanceiro}",
                DerivadoDaEmbarcacao = true,
                EquipeId = x.Id
            }));

        return custos
            .OrderBy(x => x.Categoria)
            .ThenBy(x => x.Descricao);
    }

    public async Task<CustoTorneioDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        return entidade is null ? null : ParaDto(entidade);
    }

    public async Task<CustoTorneioDto> Criar(CriarCustoTorneioDto dto)
    {
        var entidade = CustoTorneio.Criar(
            dto.TorneioId,
            dto.Categoria,
            dto.Descricao,
            dto.Quantidade,
            dto.ValorUnitario,
            dto.Responsavel,
            dto.Observacao);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarCustoTorneioDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Custo '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Custo '{id}' nao encontrado.");

        entidade.Atualizar(dto.Categoria, dto.Descricao, dto.Quantidade, dto.ValorUnitario, dto.Responsavel, dto.Observacao);
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Custo '{id}' nao encontrado.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Custo '{id}' nao encontrado.");

        await _repositorio.Remover(id);
    }

    private static CustoTorneioDto ParaDto(CustoTorneio x) => new()
    {
        Id = x.Id,
        TorneioId = x.TorneioId,
        Categoria = x.Categoria.ToString(),
        Descricao = x.Descricao,
        Quantidade = x.Quantidade,
        ValorUnitario = x.ValorUnitario,
        ValorTotal = x.ValorTotal,
        Responsavel = x.Responsavel,
        Observacao = x.Observacao
    };
}
