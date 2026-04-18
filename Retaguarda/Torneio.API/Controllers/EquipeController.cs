using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/equipes — AdminTorneio (escrita), Fiscal (leitura da própria equipe)
/// </summary>
[Authorize]
[Route("api/{slug}/equipes")]
public class EquipeController : BaseController
{
    private readonly IEquipeServico _servico;

    public EquipeController(IEquipeServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarTodos());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarEquipeDto dto)
    {
        var criado = await _servico.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarEquipeDto dto)
    {
        await _servico.Atualizar(id, dto);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost("{id:guid}/membros/{membroId:guid}")]
    public async Task<IActionResult> AdicionarMembro(Guid id, Guid membroId)
    {
        await _servico.AdicionarMembro(id, membroId);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpDelete("{id:guid}/membros/{membroId:guid}")]
    public async Task<IActionResult> RemoverMembro(Guid id, Guid membroId)
    {
        await _servico.RemoverMembro(id, membroId);
        return NoContent();
    }
}
