using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;

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
        [FromServices] IMembroServico membroServico,
        [FromServices] ICapturaServico capturaServico)
    {
        var torneioId = GetTorneioIdClaim();
        if (torneioId is null)
            return Unauthorized(new { erro = "Torneio não identificado no token." });

        var torneio = await torneioServico.ObterPorId(torneioId.Value);
        if (torneio is null)
            return NotFound(new { erro = "Torneio não encontrado." });

        var todasCapturas = (await capturaServico.ListarTodos())
            .Where(c => !c.Invalidada).ToList();

        // Aplica regra de maior captura por pescador quando configurado
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
            var equipes = (await equipeServico.ListarTodos()).ToList();
            equipesGanhadoras = equipes
                .Select(e => new
                {
                    EquipeId = e.Id,
                    NomeEquipe = e.Nome,
                    Capitao = e.Capitao,
                    TotalPontos = capturas.Where(c => c.EquipeId == e.Id).Sum(c => c.Pontuacao)
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.NomeEquipe)
                .Take(torneio.QtdGanhadores)
                .Select((x, i) => (object)new { Posicao = i + 1, x.EquipeId, x.NomeEquipe, x.Capitao, x.TotalPontos })
                .ToList();
        }

        if (torneio.PremiacaoPorMembro)
        {
            var membros = (await membroServico.ListarTodos()).ToList();
            membrosGanhadores = membros
                .Select(m => new
                {
                    MembroId = m.Id,
                    NomeMembro = m.Nome,
                    TotalPontos = capturas.Where(c => c.MembroId == m.Id).Sum(c => c.Pontuacao)
                })
                .OrderByDescending(x => x.TotalPontos).ThenBy(x => x.NomeMembro)
                .Take(torneio.QtdGanhadores)
                .Select((x, i) => (object)new { Posicao = i + 1, x.MembroId, x.NomeMembro, x.TotalPontos })
                .ToList();
        }

        return Ok(new
        {
            torneio.PremiacaoPorEquipe,
            torneio.PremiacaoPorMembro,
            EquipesGanhadoras = equipesGanhadoras,
            MembrosGanhadores = membrosGanhadores,
        });
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpGet("maiores-capturas")]
    public async Task<IActionResult> MaioresCapturas(
        [FromQuery] int quantidade = 1,
        [FromServices] ITorneioServico torneioServico = null!)
    {
        try
        {
            var torneioId = GetTorneioIdClaim();
            if (torneioId is null)
                return Unauthorized(new { erro = "Torneio não identificado no token." });

            var torneio = await torneioServico.ObterPorId(torneioId.Value);
            if (torneio is null)
                return NotFound(new { erro = "Torneio não encontrado." });

            if (torneio.TipoTorneio != nameof(TipoTorneio.Pesca))
                return BadRequest(new { erro = "O relatório de maiores capturas está disponível somente para torneios do tipo pesca." });

            var bytes = await _servico.GerarRelatorioMaioresCapturas(quantidade);
            return File(bytes, "application/pdf", $"maiores_capturas_{quantidade}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
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
