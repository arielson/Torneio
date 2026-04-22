using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[AllowAnonymous]
[Route("api/{slug}/pescador/recuperar-senha")]
public class RecuperacaoSenhaMembroController : ControllerBase
{
    private readonly TenantContext _tenantContext;
    private readonly ITorneioServico _torneioServico;
    private readonly IMembroServico _membroServico;

    public RecuperacaoSenhaMembroController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IMembroServico membroServico)
    {
        _tenantContext = tenantContext;
        _torneioServico = torneioServico;
        _membroServico = membroServico;
    }

    [HttpPost("solicitar-codigo")]
    public async Task<IActionResult> SolicitarCodigo([FromBody] SolicitarRecuperacaoSenhaMembroDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException("Torneio nao encontrado.");

        var resultado = await _membroServico.SolicitarRecuperacaoSenha(
            torneio.Id,
            torneio.NomeTorneio,
            dto,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(resultado);
    }

    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarRecuperacaoSenhaMembroDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(_tenantContext.TorneioId)
            ?? throw new KeyNotFoundException("Torneio nao encontrado.");

        await _membroServico.RedefinirSenha(
            torneio.Id,
            torneio.NomeTorneio,
            dto,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(new { mensagem = "Senha redefinida com sucesso." });
    }
}
