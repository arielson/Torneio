using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Grupo;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/grupos — AdminTorneio
/// Grupos pre-formados para o modo GrupoEquipe.
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/grupos")]
public class GrupoController : BaseController
{
    private readonly IGrupoAppServico _servico;

    public GrupoController(IGrupoAppServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> ListarTodos() =>
        Ok(await _servico.ListarTodos());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var grupo = await _servico.ObterPorId(id);
        return grupo is null ? NotFound() : Ok(grupo);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarGrupoDto dto)
    {
        var grupo = await _servico.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = grupo.Id }, grupo);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarGrupoDto dto)
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

    [HttpPost("{id:guid}/membros")]
    public async Task<IActionResult> AdicionarMembro(Guid id, [FromBody] AdicionarGrupoMembroDto dto)
    {
        await _servico.AdicionarMembro(id, dto.MembroId);
        return NoContent();
    }

    [HttpDelete("{id:guid}/membros/{grupoMembroId:guid}")]
    public async Task<IActionResult> RemoverMembro(Guid id, Guid grupoMembroId)
    {
        await _servico.RemoverMembro(grupoMembroId);
        return NoContent();
    }
}
