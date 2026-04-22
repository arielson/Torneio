using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize(Policy = "MembroTorneio")]
[Route("api/{slug}/membro/financeiro")]
public class MembroFinanceiroController : BaseController
{
    private readonly IFinanceiroTorneioServico _financeiroServico;
    private readonly TenantContext _tenantContext;

    public MembroFinanceiroController(IFinanceiroTorneioServico financeiroServico, TenantContext tenantContext)
    {
        _financeiroServico = financeiroServico;
        _tenantContext = tenantContext;
    }

    [HttpGet("cobrancas")]
    public async Task<IActionResult> Cobrancas() =>
        Ok(await _financeiroServico.ListarParcelas(_tenantContext.TorneioId, GetUserId(), false, false, null));
}
