using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/membros — AdminTorneio
/// </summary>
[Authorize]
[Route("api/{slug}/membros")]
public class MembroController : BaseController
{
    private readonly IMembroServico _servico;

    public MembroController(IMembroServico servico) => _servico = servico;

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
    public async Task<IActionResult> Criar([FromBody] CriarMembroDto dto)
    {
        var criado = await _servico.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarMembroDto dto)
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
}
