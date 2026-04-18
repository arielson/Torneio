using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/relatorios — geração de PDFs
/// Fiscal: somente sua equipe. AdminTorneio/AdminGeral: qualquer equipe ou membro.
/// </summary>
[Authorize]
[Route("api/{slug}/relatorios")]
public class RelatorioController : BaseController
{
    private readonly IRelatorioServico _servico;

    public RelatorioController(IRelatorioServico servico) => _servico = servico;

    [Authorize(Policy = "AdminTorneio")]
    [HttpGet("ganhadores")]
    public async Task<IActionResult> Ganhadores(
        [FromServices] ITorneioServico torneioServico,
        [FromServices] IEquipeServico equipeServico,
        [FromServices] ICapturaServico capturaServico)
    {
        var torneioId = GetTorneioIdClaim();
        if (torneioId is null)
            return Unauthorized(new { erro = "Torneio não identificado no token." });

        var torneio = await torneioServico.ObterPorId(torneioId.Value);
        if (torneio is null)
            return NotFound(new { erro = "Torneio não encontrado." });

        var equipes = (await equipeServico.ListarTodos()).ToList();
        var capturas = (await capturaServico.ListarTodos()).ToList();

        var ganhadores = equipes
            .Select(e => new
            {
                EquipeId = e.Id,
                NomeEquipe = e.Nome,
                Capitao = e.Capitao,
                TotalPontos = capturas
                    .Where(c => c.EquipeId == e.Id)
                    .Sum(c => c.Pontuacao)
            })
            .OrderByDescending(x => x.TotalPontos)
            .ThenBy(x => x.NomeEquipe)
            .Take(torneio.QtdGanhadores)
            .Select((x, index) => new
            {
                Posicao = index + 1,
                x.EquipeId,
                x.NomeEquipe,
                x.Capitao,
                x.TotalPontos
            });

        return Ok(ganhadores);
    }

    /// <summary>
    /// GET /api/{slug}/relatorios/equipe/{equipeId}?analitico=false
    /// </summary>
    [HttpGet("equipe/{equipeId:guid}")]
    public async Task<IActionResult> RelatorioEquipe(
        Guid equipeId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _servico.GerarRelatorioEquipe(equipeId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"equipe_{equipeId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/{slug}/relatorios/membro/{membroId}?analitico=false
    /// </summary>
    [Authorize(Policy = "AdminTorneio")]
    [HttpGet("membro/{membroId:guid}")]
    public async Task<IActionResult> RelatorioMembro(
        Guid membroId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _servico.GerarRelatorioMembro(membroId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"membro_{membroId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }
}
