using FluentValidation;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class EquipeServico : IEquipeServico
{
    private readonly IEquipeRepositorio _repositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly ITenantContext _tenantContext;
    private readonly IValidator<CriarEquipeDto> _validador;

    public EquipeServico(
        IEquipeRepositorio repositorio,
        IMembroRepositorio membroRepositorio,
        ITenantContext tenantContext,
        IValidator<CriarEquipeDto> validador)
    {
        _repositorio = repositorio;
        _membroRepositorio = membroRepositorio;
        _tenantContext = tenantContext;
        _validador = validador;
    }

    public async Task<EquipeDto?> ObterPorId(Guid id)
    {
        var entidade = await _repositorio.ObterComMembros(id);
        if (entidade is null || entidade.TorneioId != _tenantContext.TorneioId)
            return null;

        return ParaDto(entidade);
    }

    public async Task<IEnumerable<EquipeDto>> ListarTodos()
    {
        var lista = await _repositorio.ListarPorTorneio(_tenantContext.TorneioId);
        return lista.Select(ParaDto);
    }

    public async Task<EquipeDto> Criar(CriarEquipeDto dto)
    {
        await _validador.ValidateAndThrowAsync(dto);

        var entidade = Equipe.Criar(
            dto.TorneioId,
            dto.Nome, dto.Capitao, dto.FiscalId, dto.QtdVagas,
            dto.FotoUrl, dto.FotoCapitaoUrl);

        await _repositorio.Adicionar(entidade);
        return ParaDto(entidade);
    }

    public async Task Atualizar(Guid id, AtualizarEquipeDto dto)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Equipe '{id}' nao encontrada.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Equipe '{id}' nao encontrada.");

        entidade.Atualizar(dto.Nome, dto.Capitao, dto.QtdVagas, dto.FotoUrl, dto.FotoCapitaoUrl);
        await _repositorio.Atualizar(entidade);
    }

    public async Task Remover(Guid id)
    {
        var entidade = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Equipe '{id}' nao encontrada.");
        if (entidade.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Equipe '{id}' nao encontrada.");

        await _repositorio.Remover(entidade.Id);
    }

    public async Task AdicionarMembro(Guid equipeId, Guid membroId)
    {
        var equipe = await _repositorio.ObterComMembros(equipeId)
            ?? throw new KeyNotFoundException($"Equipe '{equipeId}' nao encontrada.");
        if (equipe.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Equipe '{equipeId}' nao encontrada.");

        var membro = await _membroRepositorio.ObterPorId(membroId)
            ?? throw new KeyNotFoundException($"Membro '{membroId}' nao encontrado.");
        if (membro.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Membro '{membroId}' nao encontrado.");

        equipe.AdicionarMembro(membro);
        await _repositorio.Atualizar(equipe);
    }

    public async Task RemoverMembro(Guid equipeId, Guid membroId)
    {
        var equipe = await _repositorio.ObterComMembros(equipeId)
            ?? throw new KeyNotFoundException($"Equipe '{equipeId}' nao encontrada.");
        if (equipe.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Equipe '{equipeId}' nao encontrada.");

        equipe.RemoverMembro(membroId);
        await _repositorio.Atualizar(equipe);
    }

    private static EquipeDto ParaDto(Equipe e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl,
        Capitao = e.Capitao,
        FotoCapitaoUrl = e.FotoCapitaoUrl,
        FiscalId = e.FiscalId,
        QtdVagas = e.QtdVagas,
        QtdMembros = e.Membros.Count,
        MembroIds = e.Membros.Select(m => m.Id).ToList()
    };
}
