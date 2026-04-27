using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;
using Torneio.Web.Models;

namespace Torneio.Web.Controllers;

[Route("{slug}")]
public class PublicoController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IPatrocinadorServico _patrocinadorServico;
    private readonly ICapturaServico _capturaServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;

    public PublicoController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IPatrocinadorServico patrocinadorServico,
        ICapturaServico capturaServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico)
        : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _patrocinadorServico = patrocinadorServico;
        _capturaServico = capturaServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null) return NotFound();

        ViewBag.Patrocinadores = await _patrocinadorServico.ListarPorTorneio(torneio.Id);
        if (torneio.ExibirParticipantesPublicos)
        {
            ViewBag.Participantes = (await _membroServico.ListarTodos())
                .OrderBy(m => m.Nome)
                .ToList();
        }

        if (torneio.Status is "Liberado" or "Finalizado")
        {
            var todasCapturas = (await _capturaServico.ListarTodos())
                .Where(c => !c.Invalidada)
                .ToList();

            // Se ativado, cada pescador contribui apenas com sua maior captura
            var capturas = torneio.ApenasMaiorCapturaPorPescador
                ? todasCapturas
                    .GroupBy(c => c.MembroId)
                    .Select(g => g.OrderByDescending(c => c.Pontuacao).First())
                    .ToList()
                : todasCapturas;

            var equipes  = (await _equipeServico.ListarTodos()).ToDictionary(e => e.Id);
            var membros  = (await _membroServico.ListarTodos()).ToDictionary(m => m.Id);

            // Ranking por equipe
            var rankingEquipes = capturas
                .GroupBy(c => c.EquipeId)
                .Select(g =>
                {
                    equipes.TryGetValue(g.Key, out var eq);
                    return new RankingEquipeVm
                    {
                        EquipeId       = g.Key,
                        NomeEquipe     = eq?.Nome ?? g.First().NomeEquipe,
                        FotoUrl        = eq?.FotoUrl,
                        TotalPontos    = g.Sum(c => c.Pontuacao),
                        QtdCapturas    = g.Count(),
                        PrimeiraCaptura = g.Min(c => c.DataHora)
                    };
                })
                .OrderByDescending(r => r.TotalPontos)
                .ThenBy(r => r.PrimeiraCaptura)
                .ThenBy(r => r.NomeEquipe)
                .ToList();

            for (var i = 0; i < rankingEquipes.Count; i++)
                rankingEquipes[i].Posicao = i + 1;

            // Ranking por membro
            var rankingMembros = capturas
                .GroupBy(c => c.MembroId)
                .Select(g =>
                {
                    membros.TryGetValue(g.Key, out var m);
                    return new RankingMembroVm
                    {
                        NomeMembro      = m?.Nome ?? g.First().NomeMembro,
                        FotoUrl         = m?.FotoUrl,
                        NomeEquipe      = g.First().NomeEquipe,
                        TotalPontos     = g.Sum(c => c.Pontuacao),
                        PrimeiraCaptura = g.Min(c => c.DataHora),
                        Capturas        = g.OrderByDescending(c => c.Pontuacao)
                            .Select(c => new RankingCapturaVm
                            {
                                NomeItem           = c.NomeItem,
                                TamanhoMedida      = c.TamanhoMedida,
                                FatorMultiplicador = c.FatorMultiplicador,
                                Pontuacao          = c.Pontuacao,
                                FotoUrl            = c.FotoUrl,
                                DataHora           = c.DataHora
                            }).ToList()
                    };
                })
                .OrderByDescending(r => r.TotalPontos)
                .ThenBy(r => r.PrimeiraCaptura)
                .ThenBy(r => r.NomeMembro)
                .ToList();

            for (var i = 0; i < rankingMembros.Count; i++)
                rankingMembros[i].Posicao = i + 1;

            ViewBag.RankingEquipes  = rankingEquipes;
            ViewBag.RankingMembros  = rankingMembros;
            ViewBag.UsarFator       = torneio.UsarFatorMultiplicador;
            ViewBag.MedidaCaptura   = torneio.MedidaCaptura;
            ViewBag.LabelEquipe     = torneio.LabelEquipe;
            ViewBag.LabelEquipePlural = torneio.LabelEquipePlural;
            ViewBag.LabelMembro     = torneio.LabelMembro;
            ViewBag.LabelMembroPlural = torneio.LabelMembroPlural;
            ViewBag.LabelItem       = torneio.LabelItem;
            ViewBag.LabelCaptura    = torneio.LabelCaptura;
        }

        return View(torneio);
    }
}
