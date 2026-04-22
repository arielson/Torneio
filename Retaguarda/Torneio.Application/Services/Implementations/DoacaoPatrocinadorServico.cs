using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class DoacaoPatrocinadorServico : IDoacaoPatrocinadorServico
{
    private readonly IDoacaoPatrocinadorRepositorio _repositorio;
    private readonly IPatrocinadorRepositorio _patrocinadorRepositorio;
    private readonly ITenantContext _tenantContext;

    public DoacaoPatrocinadorServico(
        IDoacaoPatrocinadorRepositorio repositorio,
        IPatrocinadorRepositorio patrocinadorRepositorio,
        ITenantContext tenantContext)
    {
        _repositorio = repositorio;
        _patrocinadorRepositorio = patrocinadorRepositorio;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<DoacaoPatrocinadorDto>> Listar(Guid torneioId) =>
        (await _repositorio.ListarPorTorneio(torneioId))
        .OrderByDescending(x => x.DataDoacao)
        .ThenBy(x => x.NomePatrocinador)
        .Select(ParaDto);

    public async Task<DoacaoPatrocinadorDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        if (entidade is null || entidade.TorneioId != _tenantContext.TorneioId)
            return null;

        return ParaDto(entidade);
    }

    public async Task<DoacaoPatrocinadorDto> Criar(CriarDoacaoPatrocinadorDto dto)
    {
        var nomePatrocinador = await ResolverNomePatrocinador(dto.PatrocinadorId, dto.NomePatrocinador);
        Validar(dto.Tipo, dto.Descricao, dto.Valor);

        var entidade = DoacaoPatrocinador.Criar(
            dto.TorneioId,
            dto.PatrocinadorId,
            nomePatrocinador,
            dto.Tipo,
            dto.Descricao,
            dto.Quantidade,
            AjustarValor(dto.Tipo, dto.Valor),
            dto.Observacao,
            dto.DataDoacao);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarDoacaoPatrocinadorDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Doacao '{id}' nao encontrada.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Doacao '{id}' nao encontrada.");

        var nomePatrocinador = await ResolverNomePatrocinador(dto.PatrocinadorId, dto.NomePatrocinador);
        Validar(dto.Tipo, dto.Descricao, dto.Valor);

        entidade.Atualizar(
            dto.PatrocinadorId,
            nomePatrocinador,
            dto.Tipo,
            dto.Descricao,
            dto.Quantidade,
            AjustarValor(dto.Tipo, dto.Valor),
            dto.Observacao,
            dto.DataDoacao);

        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Doacao '{id}' nao encontrada.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Doacao '{id}' nao encontrada.");

        await _repositorio.Remover(id);
    }

    private async Task<string> ResolverNomePatrocinador(Guid? patrocinadorId, string? nomeInformado)
    {
        if (patrocinadorId.HasValue)
        {
            var patrocinador = await _patrocinadorRepositorio.ObterPorId(patrocinadorId.Value)
                ?? throw new KeyNotFoundException($"Patrocinador '{patrocinadorId}' nao encontrado.");
            if (patrocinador.TorneioId != _tenantContext.TorneioId)
                throw new KeyNotFoundException($"Patrocinador '{patrocinadorId}' nao encontrado.");
            return patrocinador.Nome;
        }

        if (string.IsNullOrWhiteSpace(nomeInformado))
            throw new InvalidOperationException("Informe o patrocinador da doacao.");

        return nomeInformado.Trim();
    }

    private static void Validar(TipoDoacaoPatrocinador tipo, string? descricao, decimal? valor)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new InvalidOperationException("Informe a descricao da doacao.");

        if (tipo == TipoDoacaoPatrocinador.Dinheiro && (!valor.HasValue || valor.Value <= 0))
            throw new InvalidOperationException("Informe um valor maior que zero para doacoes em dinheiro.");
    }

    private static decimal? AjustarValor(TipoDoacaoPatrocinador tipo, decimal? valor) =>
        tipo == TipoDoacaoPatrocinador.Dinheiro ? valor : null;

    private static DoacaoPatrocinadorDto ParaDto(DoacaoPatrocinador x) => new()
    {
        Id = x.Id,
        TorneioId = x.TorneioId,
        PatrocinadorId = x.PatrocinadorId,
        NomePatrocinador = x.NomePatrocinador,
        Tipo = x.Tipo.ToString(),
        Descricao = x.Descricao,
        Quantidade = x.Quantidade,
        Valor = x.Valor,
        Observacao = x.Observacao,
        DataDoacao = x.DataDoacao
    };
}
