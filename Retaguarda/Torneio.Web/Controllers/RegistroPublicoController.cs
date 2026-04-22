using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.RegistroPublicoMembro;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[AllowAnonymous]
[Route("{slug}/registro-pescador")]
public class RegistroPublicoController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IRegistroPublicoMembroServico _registroServico;

    public RegistroPublicoController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IRegistroPublicoMembroServico registroServico) : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _registroServico = registroServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null)
            return NotFound();

        return View(torneio);
    }

    [HttpPost("solicitar-codigo")]
    public async Task<IActionResult> SolicitarCodigo([FromBody] SolicitarRegistroPublicoMembroDto dto)
    {
        try
        {
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId)
                ?? throw new KeyNotFoundException("Torneio nao encontrado.");

            var resultado = await _registroServico.SolicitarCodigo(
                torneio.Id,
                torneio.NomeTorneio,
                dto,
                IpAddress);

            return Json(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarRegistroPublicoMembroDto dto)
    {
        try
        {
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId)
                ?? throw new KeyNotFoundException("Torneio nao encontrado.");

            var membro = await _registroServico.ConfirmarCodigo(
                torneio.Id,
                torneio.NomeTorneio,
                dto,
                IpAddress);

            return Json(membro);
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}
