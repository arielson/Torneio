using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

[AllowAnonymous]
[Route("api/{slug}/participantes")]
public class ParticipantesPublicoController : BaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IMembroServico _membroServico;

    public ParticipantesPublicoController(
        ITorneioServico torneioServico,
        IMembroServico membroServico)
    {
        _torneioServico = torneioServico;
        _membroServico = membroServico;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromRoute] string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null)
            return NotFound(new { erro = $"Torneio '{slug}' não encontrado." });

        if (!torneio.ExibirParticipantesPublicos)
            return Ok(Array.Empty<object>());

        var participantes = (await _membroServico.ListarTodos())
            .OrderBy(m => m.Nome)
            .Select(m => new
            {
                m.Id,
                m.Nome,
                m.FotoUrl,
            })
            .ToList();

        return Ok(participantes);
    }
}
