using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[AllowAnonymous]
[Route("{slug}/pescador/recuperar-senha")]
public class RecuperacaoSenhaMembroController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IMembroServico _membroServico;

    public RecuperacaoSenhaMembroController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IMembroServico membroServico) : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _membroServico = membroServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug)
    {
        var torneio = await _torneioServico.ObterPorSlug(slug);
        if (torneio is null)
            return NotFound();
        if (!torneio.PermitirRegistroPublicoMembro)
            return NotFound();

        return View(torneio);
    }

    [HttpPost("solicitar-codigo")]
    public async Task<IActionResult> SolicitarCodigo([FromBody] SolicitarRecuperacaoSenhaMembroDto dto)
    {
        try
        {
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId)
                ?? throw new KeyNotFoundException("Torneio nao encontrado.");
            if (!torneio.PermitirRegistroPublicoMembro)
                throw new InvalidOperationException("O acesso do pescador nao esta habilitado neste torneio.");

            var resultado = await _membroServico.SolicitarRecuperacaoSenha(
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
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarRecuperacaoSenhaMembroDto dto)
    {
        try
        {
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId)
                ?? throw new KeyNotFoundException("Torneio nao encontrado.");
            if (!torneio.PermitirRegistroPublicoMembro)
                throw new InvalidOperationException("O acesso do pescador nao esta habilitado neste torneio.");

            await _membroServico.RedefinirSenha(
                torneio.Id,
                torneio.NomeTorneio,
                dto,
                IpAddress);

            return Json(new { mensagem = "Senha redefinida com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}
