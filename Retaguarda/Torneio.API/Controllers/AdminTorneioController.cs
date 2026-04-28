using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.AdminTorneio;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/admins-torneio — AdminGeral gerencia; AdminTorneio consulta
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/admins-torneio")]
public class AdminTorneioController : BaseController
{
    private readonly IAdminTorneioServico _servico;
    private readonly TenantContext _tenantContext;

    public AdminTorneioController(IAdminTorneioServico servico, TenantContext tenantContext)
    {
        _servico = servico;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarPorTorneio(_tenantContext.TorneioId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAdminTorneioDto dto)
    {
        var criado = await _servico.Criar(new CriarAdminTorneioDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Usuario = dto.Usuario,
            Senha = dto.Senha
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}/senha")]
    public async Task<IActionResult> AtualizarSenha(Guid id, [FromBody] AtualizarSenhaDto dto)
    {
        await _servico.AtualizarSenha(id, dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha(Guid id, [FromBody] RedefinirSenhaDto dto)
    {
        await _servico.RedefinirSenha(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }
}
