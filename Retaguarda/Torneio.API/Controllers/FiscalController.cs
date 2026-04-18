using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/fiscais — AdminTorneio
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/fiscais")]
public class FiscalController : BaseController
{
    private readonly IFiscalServico _servico;

    public FiscalController(IFiscalServico servico) => _servico = servico;

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
    public async Task<IActionResult> Criar([FromBody] CriarFiscalDto dto)
    {
        var criado = await _servico.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}/senha")]
    public async Task<IActionResult> AtualizarSenha(Guid id, [FromBody] AtualizarSenhaDto dto)
    {
        await _servico.AtualizarSenha(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }
}
