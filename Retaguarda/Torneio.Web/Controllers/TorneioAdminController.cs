using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/admin")]
public class TorneioAdminController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly IAnoTorneioServico _anoServico;
    private readonly IEquipeServico _equipeServico;
    private readonly IMembroServico _membroServico;
    private readonly IFiscalServico _fiscalServico;
    private readonly IItemServico _itemServico;

    public TorneioAdminController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        IAnoTorneioServico anoServico,
        IEquipeServico equipeServico,
        IMembroServico membroServico,
        IFiscalServico fiscalServico,
        IItemServico itemServico) : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _anoServico = anoServico;
        _equipeServico = equipeServico;
        _membroServico = membroServico;
        _fiscalServico = fiscalServico;
        _itemServico = itemServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        var anos = await _anoServico.ListarPorTorneio(TenantContext.TorneioId);
        ViewBag.Torneio = torneio;
        return View(anos);
    }
}
