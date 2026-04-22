using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.RegistroPublicoMembro;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[AllowAnonymous]
[Route("api/{slug}/registro-pescador")]
public class RegistroPublicoMembroController : ControllerBase
{
    private readonly TenantContext _tenantContext;
    private readonly ITorneioServico _torneioServico;
    private readonly IRegistroPublicoMembroServico _registroServico;

    public RegistroPublicoMembroController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IRegistroPublicoMembroServico registroServico)
    {
        _tenantContext = tenantContext;
        _torneioServico = torneioServico;
        _registroServico = registroServico;
    }

    [HttpPost("solicitar-codigo")]
    public async Task<IActionResult> SolicitarCodigo([FromBody] SolicitarRegistroPublicoMembroDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException("Torneio nao encontrado.");

        var resultado = await _registroServico.SolicitarCodigo(
            torneio.Id,
            torneio.NomeTorneio,
            dto,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(resultado);
    }

    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarRegistroPublicoMembroDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException("Torneio nao encontrado.");

        var membro = await _registroServico.ConfirmarCodigo(
            torneio.Id,
            torneio.NomeTorneio,
            dto,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(membro);
    }
}
