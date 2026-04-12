using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/membros — AdminTorneio
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/membros")]
public class MembroController : BaseController
{
    private readonly IMembroServico _servico;

    public MembroController(IMembroServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid anoTorneioId) =>
        Ok(await _servico.ListarPorAnoTorneio(anoTorneioId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarMembroDto dto)
    {
        var criado = await _servico.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarMembroDto dto)
    {
        await _servico.Atualizar(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }
}
