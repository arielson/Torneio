using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.AdminGeral;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/admin/admins-geral — AdminGeral
/// </summary>
[Authorize(Policy = "AdminGeral")]
[Route("api/admin/admins-geral")]
public class AdminGeralController : BaseController
{
    private readonly IAdminGeralServico _servico;

    public AdminGeralController(IAdminGeralServico servico) => _servico = servico;

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
    public async Task<IActionResult> Criar([FromBody] CriarAdminGeralDto dto)
    {
        var criado = await _servico.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
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
