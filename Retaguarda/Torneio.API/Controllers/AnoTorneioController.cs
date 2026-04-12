using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.AnoTorneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/anos — AdminTorneio
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/anos")]
public class AnoTorneioController : BaseController
{
    private readonly IAnoTorneioServico _servico;
    private readonly TenantContext _tenantContext;

    public AnoTorneioController(IAnoTorneioServico servico, TenantContext tenantContext)
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
    public async Task<IActionResult> Criar([FromBody] CriarAnoTorneioDto dto)
    {
        var criado = await _servico.Criar(new CriarAnoTorneioDto { TorneioId = _tenantContext.TorneioId, Ano = dto.Ano });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}/liberar")]
    public async Task<IActionResult> Liberar(Guid id)
    {
        await _servico.Liberar(id);
        return NoContent();
    }

    [HttpPut("{id:guid}/finalizar")]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        await _servico.Finalizar(id);
        return NoContent();
    }

    [HttpPut("{id:guid}/reabrir")]
    public async Task<IActionResult> Reabrir(Guid id)
    {
        await _servico.Reabrir(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/replicar")]
    public async Task<IActionResult> Replicar(Guid id, [FromQuery] int novoAno)
    {
        var criado = await _servico.ReplicarAno(id, novoAno);
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }
}
