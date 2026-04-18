using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Item;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/itens — AdminTorneio
/// </summary>
[Authorize]
[Route("api/{slug}/itens")]
public class ItemController : BaseController
{
    private readonly IItemServico _servico;
    private readonly TenantContext _tenantContext;

    public ItemController(IItemServico servico, TenantContext tenantContext)
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

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarItemDto dto)
    {
        var criado = await _servico.Criar(new CriarItemDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Comprimento = dto.Comprimento,
            FatorMultiplicador = dto.FatorMultiplicador,
            FotoUrl = dto.FotoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarItemDto dto)
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
