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

    /// <summary>
    /// GET /api/{slug}/relatorios/equipe/{equipeId}?anoTorneioId=...&amp;analitico=false
    /// </summary>
    [HttpGet("equipe/{equipeId:guid}")]
    public async Task<IActionResult> RelatorioEquipe(
        Guid equipeId,
        [FromQuery] Guid anoTorneioId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _servico.GerarRelatorioEquipe(anoTorneioId, equipeId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"equipe_{equipeId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/{slug}/relatorios/membro/{membroId}?anoTorneioId=...&amp;analitico=false
    /// </summary>
    [Authorize(Policy = "AdminTorneio")]
    [HttpGet("membro/{membroId:guid}")]
    public async Task<IActionResult> RelatorioMembro(
        Guid membroId,
        [FromQuery] Guid anoTorneioId,
        [FromQuery] bool analitico = false)
    {
        try
        {
            var bytes = await _servico.GerarRelatorioMembro(anoTorneioId, membroId, analitico);
            var tipo = analitico ? "analitico" : "sintetico";
            return File(bytes, "application/pdf", $"membro_{membroId}_{tipo}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }
}
