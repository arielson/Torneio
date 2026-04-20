using Torneio.Application.DTOs.Grupo;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class GrupoAppServico : IGrupoAppServico
{
    private readonly IGrupoRepositorio _repositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly ITenantContext _tenantContext;

    public GrupoAppServico(
        IGrupoRepositorio repositorio,
        IMembroRepositorio membroRepositorio,
        ITenantContext tenantContext)
    {
        _repositorio = repositorio;
        _membroRepositorio = membroRepositorio;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<GrupoDto>> ListarTodos()
    {
        var grupos = await _repositorio.ListarComMembros();
        return grupos.OrderBy(g => g.Nome).Select(ParaDto);
    }

    public async Task<GrupoDto?> ObterPorId(Guid id)
    {
        var grupo = await _repositorio.ObterComMembros(id);
        return grupo is null ? null : ParaDto(grupo);
    }

    public async Task<GrupoDto> Criar(CriarGrupoDto dto)
    {
        var grupo = Grupo.Criar(_tenantContext.TorneioId, dto.Nome);
        await _repositorio.Adicionar(grupo);
        return ParaDto(grupo);
    }

    public async Task Atualizar(Guid id, AtualizarGrupoDto dto)
    {
        var grupo = await _repositorio.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Grupo '{id}' não encontrado.");
        grupo.Renomear(dto.Nome);
        await _repositorio.Atualizar(grupo);
    }

    public async Task Remover(Guid id)
    {
        var grupo = await _repositorio.ObterComMembros(id)
            ?? throw new KeyNotFoundException($"Grupo '{id}' não encontrado.");
        if (grupo.Membros.Count > 0)
            throw new InvalidOperationException("Remova todos os membros do grupo antes de excluí-lo.");
        await _repositorio.Remover(id);
    }

    public async Task AdicionarMembro(Guid grupoId, Guid membroId)
    {
        var grupo = await _repositorio.ObterComMembros(grupoId)
            ?? throw new KeyNotFoundException($"Grupo '{grupoId}' não encontrado.");

        var membro = await _membroRepositorio.ObterPorId(membroId)
            ?? throw new KeyNotFoundException($"Membro '{membroId}' não encontrado.");

        if (await _repositorio.MembroJaEmGrupo(membroId, excluirGrupoId: grupoId))
            throw new InvalidOperationException($"O membro '{membro.Nome}' já pertence a outro grupo.");

        var grupoMembro = GrupoMembro.Criar(grupoId, membroId);
        grupo.AdicionarMembro(grupoMembro);
        await _repositorio.AdicionarMembro(grupoMembro);
    }

    public async Task RemoverMembro(Guid grupoMembroId)
    {
        await _repositorio.RemoverMembro(grupoMembroId);
    }

    private static GrupoDto ParaDto(Grupo g) => new()
    {
        Id = g.Id,
        Nome = g.Nome,
        Membros = g.Membros
            .Select(m => new GrupoMembroDto
            {
                Id = m.Id,
                MembroId = m.MembroId,
                NomeMembro = m.Membro?.Nome ?? string.Empty
            })
            .OrderBy(m => m.NomeMembro)
            .ToList()
    };
}
