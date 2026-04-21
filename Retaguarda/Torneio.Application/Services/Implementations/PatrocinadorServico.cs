using FluentValidation;
using Torneio.Application.DTOs.Patrocinador;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class PatrocinadorServico : IPatrocinadorServico
{
    private readonly IPatrocinadorRepositorio _repositorio;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<CriarPatrocinadorDto> _validador;

    public PatrocinadorServico(
        IPatrocinadorRepositorio repositorio,
        ITenantContext tenantContext,
        IValidator<CriarPatrocinadorDto> validador)
    {
        _repositorio = repositorio;
        _tenantContext = tenantContext;
        _validador = validador;
    }

    public async Task<PatrocinadorDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id);
        if (entidade is null || entidade.TorneioId != _tenantContext.TorneioId)
        {
            return null;
        }

        return ParaDto(entidade);
    }

    public async Task<IEnumerable<PatrocinadorDto>> ListarPorTorneio(Guid torneioId)
    {
        var lista = await _repositorio.ListarPorTorneio(torneioId);
        return lista.Select(ParaDto);
    }

    public async Task<PatrocinadorDto> Criar(CriarPatrocinadorDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var entidade = Patrocinador.Criar(
            dto.TorneioId,
            dto.Nome,
            dto.FotoUrl,
            Limpar(dto.Instagram),
            Limpar(dto.Site),
            Limpar(dto.Zap),
            dto.ExibirNaTelaInicial,
            dto.ExibirNosRelatorios);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarPatrocinadorDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Patrocinador '{id}' nao encontrado.");

        if (entidade.TorneioId != _tenantContext.TorneioId)
        {
            throw new KeyNotFoundException($"Patrocinador '{id}' nao encontrado.");
        }

        entidade.Atualizar(
            dto.Nome,
            Limpar(dto.FotoUrl),
            Limpar(dto.Instagram),
            Limpar(dto.Site),
            Limpar(dto.Zap),
            dto.ExibirNaTelaInicial,
            dto.ExibirNosRelatorios);

        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Patrocinador '{id}' nao encontrado.");

        if (entidade.TorneioId != _tenantContext.TorneioId)
        {
            throw new KeyNotFoundException($"Patrocinador '{id}' nao encontrado.");
        }

        await _repositorio.Remover(entidade.Id);
    }

    private static PatrocinadorDto ParaDto(Patrocinador e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl,
        Instagram = e.Instagram,
        Site = e.Site,
        Zap = e.Zap,
        ExibirNaTelaInicial = e.ExibirNaTelaInicial,
        ExibirNosRelatorios = e.ExibirNosRelatorios
    };

    private static string? Limpar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
