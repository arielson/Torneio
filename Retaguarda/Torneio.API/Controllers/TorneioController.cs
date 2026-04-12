using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/admin/torneiros — AdminGeral
/// </summary>
[Authorize(Policy = "AdminGeral")]
[Route("api/admin/torneiros")]
public class TorneioController : BaseController
{
    private readonly ITorneioServico _servico;

    public TorneioController(ITorneioServico servico) => _servico = servico;

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarTodos());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTorneioDto dto)
    {
        var criado = await _servico.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTorneioDto dto)
    {
        await _servico.Atualizar(id, dto);
        return NoContent();
    }

    [HttpPost("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid id)
    {
        await _servico.Ativar(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        await _servico.Desativar(id);
        return NoContent();
    }
}
