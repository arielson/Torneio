using Torneio.Application.DTOs.Sorteio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class SorteioGrupoAppServico : ISorteioGrupoAppServico
{
    private readonly ISorteioGrupoServico _sorteioServico;
    private readonly ISorteioGrupoRepositorio _sorteioRepositorio;
    private readonly IGrupoRepositorio _grupoRepositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly ITorneioRepositorio _torneioRepositorio;
    private readonly ITenantContext _tenantContext;

    public SorteioGrupoAppServico(
        ISorteioGrupoServico sorteioServico,
        ISorteioGrupoRepositorio sorteioRepositorio,
        IGrupoRepositorio grupoRepositorio,
        IEquipeRepositorio equipeRepositorio,
        ITorneioRepositorio torneioRepositorio,
        ITenantContext tenantContext)
    {
        _sorteioServico = sorteioServico;
        _sorteioRepositorio = sorteioRepositorio;
        _grupoRepositorio = grupoRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _torneioRepositorio = torneioRepositorio;
        _tenantContext = tenantContext;
    }

    public async Task<SorteioGrupoPreCondicoesDto> VerificarPreCondicoes()
    {
        var torneio = await _torneioRepositorio.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException($"Torneio '{_tenantContext.TorneioId}' não encontrado.");

        var grupos = (await _grupoRepositorio.ListarComMembros()).ToList();
        var equipes = (await _equipeRepositorio.ListarTodos()).ToList();

        string? erro = null;
        if (grupos.Count < 2)
            erro = $"São necessários pelo menos 2 grupos para realizar o sorteio. Cadastrados: {grupos.Count}.";
        else if (equipes.Count < 2)
            erro = $"São necessárias pelo menos 2 {torneio.LabelEquipePlural.ToLower()} para realizar o sorteio. Cadastradas: {equipes.Count}.";
        else if (grupos.Count != equipes.Count)
            erro = $"O número de grupos ({grupos.Count}) deve ser igual ao número de {torneio.LabelEquipePlural.ToLower()} ({equipes.Count}).";

        return new SorteioGrupoPreCondicoesDto
        {
            QtdGrupos = grupos.Count,
            QtdEquipes = equipes.Count,
            Valido = erro is null,
            MensagemErro = erro
        };
    }

    public async Task<IEnumerable<SorteioGrupoDto>> RealizarSorteio(RealizarSorteioGrupoDto? filtro = null)
    {
        var torneio = await _torneioRepositorio.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException($"Torneio '{_tenantContext.TorneioId}' não encontrado.");

        var todosGrupos = (await _grupoRepositorio.ListarComMembros()).ToList();
        var todasEquipes = (await _equipeRepositorio.ListarTodos()).ToList();

        var grupos = filtro?.GrupoIds is { Count: > 0 }
            ? todosGrupos.Where(g => filtro.GrupoIds.Contains(g.Id)).ToList()
            : todosGrupos;

        var equipes = filtro?.EquipeIds is { Count: > 0 }
            ? todasEquipes.Where(e => filtro.EquipeIds.Contains(e.Id)).ToList()
            : todasEquipes;

        if (grupos.Count < 2)
            throw new InvalidOperationException(
                $"São necessários pelo menos 2 grupos. Selecionados: {grupos.Count}.");

        if (equipes.Count < 2)
            throw new InvalidOperationException(
                $"São necessárias pelo menos 2 {torneio.LabelEquipePlural.ToLower()}. Selecionadas: {equipes.Count}.");

        if (grupos.Count != equipes.Count)
            throw new InvalidOperationException(
                $"O número de grupos selecionados ({grupos.Count}) deve ser igual ao número de {torneio.LabelEquipePlural.ToLower()} selecionadas ({equipes.Count}). Ajuste a seleção.");

        var resultado = await _sorteioServico.RealizarSorteioAsync(_tenantContext.TorneioId, grupos, equipes);
        return ParaDtoLista(resultado, grupos, equipes);
    }

    public async Task ConfirmarSorteio(IEnumerable<ConfirmarSorteioGrupoItemDto> itens)
    {
        var existente = await _sorteioRepositorio.ListarPorTorneio(_tenantContext.TorneioId);
        if (existente.Any())
            throw new InvalidOperationException("O sorteio já foi realizado. Limpe o resultado atual antes de sortear novamente.");

        var entidades = itens.Select(i =>
            SorteioGrupo.Criar(_tenantContext.TorneioId, i.GrupoId, i.EquipeId, i.Posicao));
        await _sorteioServico.SalvarSorteioAsync(entidades);
    }

    public async Task<IEnumerable<SorteioGrupoDto>> ObterResultado()
    {
        var resultado = (await _sorteioRepositorio.ListarPorTorneio(_tenantContext.TorneioId)).ToList();
        if (!resultado.Any()) return Enumerable.Empty<SorteioGrupoDto>();

        var grupos = (await _grupoRepositorio.ListarComMembros()).ToDictionary(g => g.Id);
        var equipes = (await _equipeRepositorio.ListarTodos()).ToDictionary(e => e.Id);

        return ParaDtoLista(resultado, grupos, equipes);
    }

    public async Task AjustarPosicao(Guid sorteioGrupoId, int novaPosicao)
    {
        var entidade = await _sorteioRepositorio.ObterPorId(sorteioGrupoId)
            ?? throw new KeyNotFoundException($"SorteioGrupo '{sorteioGrupoId}' não encontrado.");
        entidade.AjustarPosicao(novaPosicao);
        await _sorteioRepositorio.Atualizar(entidade);
    }

    public async Task LimparSorteio() =>
        await _sorteioServico.LimparSorteioAsync(_tenantContext.TorneioId);

    private static IEnumerable<SorteioGrupoDto> ParaDtoLista(
        IEnumerable<SorteioGrupo> lista,
        IEnumerable<Grupo> grupos,
        IEnumerable<Equipe> equipes)
    {
        var grupoMap = grupos.ToDictionary(g => g.Id);
        var equipeMap = equipes.ToDictionary(e => e.Id);
        return ParaDtoLista(lista, grupoMap, equipeMap);
    }

    private static IEnumerable<SorteioGrupoDto> ParaDtoLista(
        IEnumerable<SorteioGrupo> lista,
        Dictionary<Guid, Grupo> grupoMap,
        Dictionary<Guid, Equipe> equipeMap)
    {
        return lista.Select(s =>
        {
            grupoMap.TryGetValue(s.GrupoId, out var grupo);
            equipeMap.TryGetValue(s.EquipeId, out var equipe);
            return new SorteioGrupoDto
            {
                Id = s.Id,
                GrupoId = s.GrupoId,
                NomeGrupo = grupo?.Nome ?? string.Empty,
                EquipeId = s.EquipeId,
                NomeEquipe = equipe?.Nome ?? string.Empty,
                Posicao = s.Posicao,
                NomesMembros = grupo?.Membros
                    .Select(m => m.Membro?.Nome ?? string.Empty)
                    .OrderBy(n => n)
                    .ToList() ?? new()
            };
        }).OrderBy(r => r.Posicao);
    }
}
