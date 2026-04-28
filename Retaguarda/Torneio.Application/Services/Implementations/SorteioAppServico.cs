using Torneio.Application.DTOs.Sorteio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Application.Services.Implementations;

public class SorteioAppServico : ISorteioAppServico
{
    private readonly ISorteioServico _sorteioServico;
    private readonly ISorteioEquipeRepositorio _sorteioRepositorio;
    private readonly IEquipeRepositorio _equipeRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly ITorneioRepositorio _torneioRepositorio;
    private readonly ITenantContext _tenantContext;

    public SorteioAppServico(
        ISorteioServico sorteioServico,
        ISorteioEquipeRepositorio sorteioRepositorio,
        IEquipeRepositorio equipeRepositorio,
        IMembroRepositorio membroRepositorio,
        ITorneioRepositorio torneioRepositorio,
        ITenantContext tenantContext)
    {
        _sorteioServico = sorteioServico;
        _sorteioRepositorio = sorteioRepositorio;
        _equipeRepositorio = equipeRepositorio;
        _membroRepositorio = membroRepositorio;
        _torneioRepositorio = torneioRepositorio;
        _tenantContext = tenantContext;
    }

    public async Task<SorteioPreCondicoesDto> VerificarPreCondicoes()
    {
        var torneio = await _torneioRepositorio.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException($"Torneio '{_tenantContext.TorneioId}' não encontrado.");

        var equipes    = (await _equipeRepositorio.ListarTodos()).ToList();
        var membros    = (await _membroRepositorio.ListarTodos()).ToList();
        var totalVagas = equipes.Sum(e => e.QtdVagas);

        string? erro = null;
        if (equipes.Count < 2)
            erro = $"São necessárias pelo menos 2 {torneio.LabelEquipePlural.ToLower()} para realizar o sorteio. Cadastradas: {equipes.Count}.";
        else if (membros.Count < totalVagas)
            erro = $"Número de {torneio.LabelMembroPlural.ToLower()} insuficiente. Vagas totais: {totalVagas}, {torneio.LabelMembroPlural.ToLower()} cadastrados: {membros.Count}.";

        return new SorteioPreCondicoesDto
        {
            QtdEquipes = equipes.Count,
            TotalVagas = totalVagas,
            QtdMembros = membros.Count,
            Valido     = erro is null,
            MensagemErro = erro,
        };
    }

    public async Task<IEnumerable<SorteioEquipeDto>> RealizarSorteio(RealizarSorteioDto? filtro = null)
    {
        var torneio = await _torneioRepositorio.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException($"Torneio '{_tenantContext.TorneioId}' não encontrado.");

        // Aplica filtros de seleção — null significa "todos"
        var todasEquipes = (await _equipeRepositorio.ListarTodos()).ToList();
        var todosMembros = (await _membroRepositorio.ListarTodos()).ToList();

        var equipes = filtro?.EquipeIds is { Count: > 0 }
            ? todasEquipes.Where(e => filtro.EquipeIds.Contains(e.Id)).ToList()
            : todasEquipes;

        var membros = filtro?.MembroIds is { Count: > 0 }
            ? todosMembros.Where(m => filtro.MembroIds.Contains(m.Id)).ToList()
            : todosMembros;

        var totalVagas = equipes.Sum(e => e.QtdVagas);

        if (equipes.Count < 2)
            throw new InvalidOperationException(
                $"São necessárias pelo menos 2 {torneio.LabelEquipePlural.ToLower()} para realizar o sorteio. Selecionadas: {equipes.Count}.");

        if (membros.Count < totalVagas)
            throw new InvalidOperationException(
                $"Número de {torneio.LabelMembroPlural.ToLower()} insuficiente: {totalVagas} vaga{(totalVagas != 1 ? "s" : "")} disponível{(totalVagas != 1 ? "is" : "")}, {membros.Count} selecionado{(membros.Count != 1 ? "s" : "")}. Selecione mais {torneio.LabelMembroPlural.ToLower()} ou reduza as {torneio.LabelEquipePlural.ToLower()}.");

        if (membros.Count > totalVagas)
            throw new InvalidOperationException(
                $"Número de {torneio.LabelMembroPlural.ToLower()} excede as vagas disponíveis: {totalVagas} vaga{(totalVagas != 1 ? "s" : "")}, {membros.Count} selecionado{(membros.Count != 1 ? "s" : "")}. {membros.Count - totalVagas} {torneio.LabelMembroPlural.ToLower()} ficariam de fora. Ajuste a seleção.");

        // Apenas calcula em memória — não salva no banco
        var resultado = await _sorteioServico.RealizarSorteioAsync(_tenantContext.TorneioId, equipes, membros);
        return await ParaDtoLista(resultado);
    }

    public async Task ConfirmarSorteio(IEnumerable<ConfirmarSorteioItemDto> itens)
    {
        var sorteioExistente = await _sorteioRepositorio.ListarPorTorneio(_tenantContext.TorneioId);
        if (sorteioExistente.Any())
            throw new InvalidOperationException("O sorteio já foi realizado. Limpe o resultado atual antes de sortear novamente.");

        var itensList = itens.ToList();
        var entidades = itensList.Select(i =>
            SorteioEquipe.Criar(_tenantContext.TorneioId, i.EquipeId, i.MembroId, i.Posicao));
        await _sorteioServico.SalvarSorteioAsync(entidades);

        // Vincular membros às equipes conforme resultado confirmado
        var equipes = (await _equipeRepositorio.ListarTodos()).ToDictionary(e => e.Id);
        var membros = (await _membroRepositorio.ListarTodos()).ToDictionary(m => m.Id);

        foreach (var item in itensList)
        {
            if (!equipes.TryGetValue(item.EquipeId, out var equipe)) continue;
            if (!membros.TryGetValue(item.MembroId, out var membro)) continue;
            if (equipe.Membros.Any(m => m.Id == item.MembroId)) continue;
            equipe.AdicionarMembro(membro);
            await _equipeRepositorio.Atualizar(equipe);
        }
    }

    public async Task<IEnumerable<SorteioEquipeDto>> ObterResultado()
    {
        var resultado = await _sorteioRepositorio.ListarPorTorneio(_tenantContext.TorneioId);
        return await ParaDtoLista(resultado);
    }

    public async Task AjustarPosicao(Guid sorteioEquipeId, int novaPosicao)
    {
        var entidade = await _sorteioRepositorio.ObterPorId(sorteioEquipeId)
            ?? throw new KeyNotFoundException($"SorteioEquipe '{sorteioEquipeId}' não encontrado.");
        entidade.AjustarPosicao(novaPosicao);
        await _sorteioRepositorio.Atualizar(entidade);
    }

    public async Task LimparSorteio()
    {
        // Desvincular os membros que foram alocados pelo sorteio
        var resultado = await _sorteioRepositorio.ListarPorTorneio(_tenantContext.TorneioId);
        var equipeIds = resultado.Select(r => r.EquipeId).Distinct().ToHashSet();
        var equipes = (await _equipeRepositorio.ListarTodos())
            .Where(e => equipeIds.Contains(e.Id))
            .ToDictionary(e => e.Id);

        foreach (var sorteio in resultado)
        {
            if (!equipes.TryGetValue(sorteio.EquipeId, out var equipe)) continue;
            if (!equipe.Membros.Any(m => m.Id == sorteio.MembroId)) continue;
            equipe.RemoverMembro(sorteio.MembroId);
            await _equipeRepositorio.Atualizar(equipe);
        }

        await _sorteioServico.LimparSorteioAsync(_tenantContext.TorneioId);
    }

    private async Task<IEnumerable<SorteioEquipeDto>> ParaDtoLista(IEnumerable<SorteioEquipe> lista)
    {
        var result = new List<SorteioEquipeDto>();
        foreach (var s in lista)
        {
            var equipe = await _equipeRepositorio.ObterPorId(s.EquipeId);
            var membro = await _membroRepositorio.ObterPorId(s.MembroId);
            result.Add(new SorteioEquipeDto
            {
                Id = s.Id,
                EquipeId = s.EquipeId,
                NomeEquipe = equipe?.Nome ?? string.Empty,
                MembroId = s.MembroId,
                NomeMembro = membro?.Nome ?? string.Empty,
                Posicao = s.Posicao
            });
        }
        return result.OrderBy(r => r.Posicao);
    }
}
