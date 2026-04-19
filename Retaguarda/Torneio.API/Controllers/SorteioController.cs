using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.API.Models;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/sorteio — AdminTorneio
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/sorteio")]
public class SorteioController : BaseController
{
    private readonly ISorteioAppServico _servico;

    public SorteioController(ISorteioAppServico servico) => _servico = servico;

    [HttpGet("pre-condicoes")]
    public async Task<IActionResult> PreCondicoes() =>
        Ok(await _servico.VerificarPreCondicoes());

    [HttpGet]
    public async Task<IActionResult> ObterResultado() =>
        Ok(await _servico.ObterResultado());

    [HttpPost]
    public async Task<IActionResult> Realizar() =>
        Ok(await _servico.RealizarSorteio());

    [HttpPut("{id:guid}/posicao")]
    public async Task<IActionResult> AjustarPosicao(Guid id, [FromBody] AjustarPosicaoDto dto)
    {
        await _servico.AjustarPosicao(id, dto.Posicao);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Limpar()
    {
        await _servico.LimparSorteio();
        return NoContent();
    }
}
