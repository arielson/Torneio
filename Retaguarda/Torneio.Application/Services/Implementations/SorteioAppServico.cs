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

    public async Task<IEnumerable<SorteioEquipeDto>> RealizarSorteio()
    {
        var preCondicoes = await VerificarPreCondicoes();
        if (!preCondicoes.Valido)
            throw new InvalidOperationException(preCondicoes.MensagemErro!);

        // Apenas calcula em memória — não salva no banco
        var resultado = await _sorteioServico.RealizarSorteioAsync(_tenantContext.TorneioId);
        return await ParaDtoLista(resultado);
    }

    public async Task ConfirmarSorteio(IEnumerable<ConfirmarSorteioItemDto> itens)
    {
        var sorteioExistente = await _sorteioRepositorio.ListarPorTorneio(_tenantContext.TorneioId);
        if (sorteioExistente.Any())
            throw new InvalidOperationException("O sorteio já foi realizado. Limpe o resultado atual antes de sortear novamente.");

        var entidades = itens.Select(i =>
            SorteioEquipe.Criar(_tenantContext.TorneioId, i.EquipeId, i.MembroId, i.Posicao));
        await _sorteioServico.SalvarSorteioAsync(entidades);
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
