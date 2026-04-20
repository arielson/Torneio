using FluentValidation;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.DTOs.Membro;
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
            dto.Nome,
            dto.Capitao,
            dto.QtdVagas,
            dto.FotoUrl,
            dto.FotoCapitaoUrl);

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

    public async Task<(EquipeDto Origem, EquipeDto Destino, MembroDto Membro)> ReorganizarMembroEmergencia(
        Guid membroId,
        Guid equipeDestinoId)
    {
        var membro = await _membroRepositorio.ObterPorId(membroId)
            ?? throw new KeyNotFoundException($"Membro '{membroId}' nao encontrado.");
        if (membro.TorneioId != _tenantContext.TorneioId)
            throw new KeyNotFoundException($"Membro '{membroId}' nao encontrado.");

        var equipes = (await _repositorio.ListarPorTorneio(_tenantContext.TorneioId)).ToList();
        var equipeDestino = equipes.FirstOrDefault(e => e.Id == equipeDestinoId)
            ?? throw new KeyNotFoundException($"Equipe '{equipeDestinoId}' nao encontrada.");

        if (equipeDestino.Membros.Any(m => m.Id == membroId))
            throw new InvalidOperationException("O membro ja esta vinculado a embarcacao de destino.");

        var equipesOrigem = equipes
            .Where(e => e.Id != equipeDestinoId && e.Membros.Any(m => m.Id == membroId))
            .ToList();

        if (equipesOrigem.Count == 0)
            throw new InvalidOperationException("O membro informado nao esta vinculado a nenhuma embarcacao de origem.");

        if (equipeDestino.Membros.Count >= equipeDestino.QtdVagas)
            throw new InvalidOperationException("A embarcacao de destino nao possui vagas disponiveis.");

        foreach (var equipeOrigem in equipesOrigem)
        {
            equipeOrigem.RemoverMembro(membroId);
            await _repositorio.Atualizar(equipeOrigem);
        }

        equipeDestino.AdicionarMembro(membro);
        await _repositorio.Atualizar(equipeDestino);

        var origemPrincipal = equipesOrigem.First();
        return (ParaDto(origemPrincipal), ParaDto(equipeDestino), ParaMembroDto(membro));
    }

    private static EquipeDto ParaDto(Equipe e) => new()
    {
        Id = e.Id,
        TorneioId = e.TorneioId,
        Nome = e.Nome,
        FotoUrl = e.FotoUrl,
        Capitao = e.Capitao,
        FotoCapitaoUrl = e.FotoCapitaoUrl,
        QtdVagas = e.QtdVagas,
        QtdMembros = e.Membros.Count,
        MembroIds = e.Membros.Select(m => m.Id).ToList(),
        FiscalIds = e.Fiscais.Select(f => f.FiscalId).Distinct().ToList()
    };

    private static MembroDto ParaMembroDto(Membro m) => new()
    {
        Id = m.Id,
        TorneioId = m.TorneioId,
        Nome = m.Nome,
        FotoUrl = m.FotoUrl
    };
}
