using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "MembroTorneio")]
[Route("{slug}/minhas-cobrancas")]
public class MembroFinanceiroController : TorneioBaseController
{
    private readonly IFinanceiroTorneioServico _financeiroServico;
    private readonly ITorneioServico _torneioServico;

    public MembroFinanceiroController(
        TenantContext tenantContext,
        IFinanceiroTorneioServico financeiroServico,
        ITorneioServico torneioServico) : base(tenantContext)
    {
        _financeiroServico = financeiroServico;
        _torneioServico = torneioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        var cobrancas = await _financeiroServico.ListarParcelas(TenantContext.TorneioId, GetUserId(), false, false, null);
        return View(cobrancas);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("sub")?.Value
            ?? throw new InvalidOperationException("Usuario autenticado invalido.");
        return Guid.Parse(claim);
    }
}
