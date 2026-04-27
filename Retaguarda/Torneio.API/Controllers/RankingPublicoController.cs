using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// GET /api/{slug}/ranking — público, sem auth
/// Retorna ranking de equipes e/ou membros quando o torneio está Liberado ou Finalizado.
/// </summary>
[AllowAnonymous]
[Route("api/{slug}/ranking")]
public class RankingPublicoController : BaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly ICapturaServico _capturaServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;

    public RankingPublicoController(
        ITorneioServico torneioServico,
        ICapturaServico capturaServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico)
    {
        _torneioServico = torneioServico;
        _capturaServico = capturaServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null)
            return NotFound(new { erro = $"Torneio '{slug}' não encontrado." });

        if (torneio.Status is not ("Liberado" or "Finalizado"))
            return Ok(new { disponivel = false, equipesGanhadoras = Array.Empty<object>(), membrosGanhadores = Array.Empty<object>() });

        var todasCapturas = (await _capturaServico.ListarTodos())
            .Where(c => !c.Invalidada).ToList();

        var capturas = torneio.ApenasMaiorCapturaPorPescador
            ? todasCapturas
                .GroupBy(c => c.MembroId)
                .Select(g => g.OrderByDescending(c => c.Pontuacao).First())
                .ToList()
            : todasCapturas;

        IEnumerable<object> equipesGanhadoras = [];
        IEnumerable<object> membrosGanhadores = [];

        if (torneio.PremiacaoPorEquipe)
        {
            var equipes = (await _equipeServico.ListarTodos()).ToDictionary(e => e.Id);
            equipesGanhadoras = capturas
                .GroupBy(c => c.EquipeId)
                .Select(g =>
                {
                    equipes.TryGetValue(g.Key, out var eq);
                    return new
                    {
                        EquipeId        = g.Key,
                        NomeEquipe      = eq?.Nome ?? g.First().NomeEquipe,
                        FotoUrl         = eq?.FotoUrl,
                        TotalPontos     = g.Sum(c => c.Pontuacao),
                        QtdCapturas     = g.Count(),
                        PrimeiraCaptura = g.Min(c => c.DataHora)
                    };
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.PrimeiraCaptura).ThenBy(x => x.NomeEquipe)
                .Select((x, i) => (object)new { Posicao = i + 1, x.EquipeId, x.NomeEquipe, x.FotoUrl, x.TotalPontos, x.QtdCapturas })
                .ToList();
        }

        if (torneio.PremiacaoPorMembro)
        {
            var membros = (await _membroServico.ListarTodos()).ToDictionary(m => m.Id);

            // Para membro, sempre usa todas as capturas válidas para detalhar (a filtragem por "maior" já foi aplicada acima)
            membrosGanhadores = capturas
                .GroupBy(c => c.MembroId)
                .Select(g =>
                {
                    membros.TryGetValue(g.Key, out var m);
                    return new
                    {
                        MembroId        = g.Key,
                        NomeMembro      = m?.Nome ?? g.First().NomeMembro,
                        FotoUrl         = m?.FotoUrl,
                        NomeEquipe      = g.First().NomeEquipe,
                        TotalPontos     = g.Sum(c => c.Pontuacao),
                        PrimeiraCaptura = g.Min(c => c.DataHora),
                        Capturas        = g.OrderByDescending(c => c.Pontuacao).Select(c => new
                        {
                            c.NomeItem,
                            c.TamanhoMedida,
                            c.FatorMultiplicador,
                            c.Pontuacao,
                            c.FotoUrl,
                            DataHora = c.DataHora.ToString("dd/MM HH:mm")
                        }).ToList()
                    };
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.PrimeiraCaptura).ThenBy(x => x.NomeMembro)
                .Select((x, i) => (object)new { Posicao = i + 1, x.MembroId, x.NomeMembro, x.FotoUrl, x.NomeEquipe, x.TotalPontos, x.Capturas })
                .ToList();
        }

        return Ok(new
        {
            disponivel = true,
            torneio.PremiacaoPorEquipe,
            torneio.PremiacaoPorMembro,
            torneio.ApenasMaiorCapturaPorPescador,
            torneio.UsarFatorMultiplicador,
            torneio.MedidaCaptura,
            EquipesGanhadoras = equipesGanhadoras,
            MembrosGanhadores = membrosGanhadores,
        });
    }
}
