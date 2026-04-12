using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

public abstract class TorneioBaseController : Controller
{
    protected readonly TenantContext TenantContext;

    protected TorneioBaseController(TenantContext tenantContext)
    {
        TenantContext = tenantContext;
    }

    protected string Slug => RouteData.Values["slug"]?.ToString() ?? string.Empty;

    // Carrega ViewBag.Torneio e ViewBag.StorageBaseUrl antes de qualquer action.
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (TenantContext.TorneioId != Guid.Empty)
        {
            var torneioServico = HttpContext.RequestServices.GetRequiredService<ITorneioServico>();
            ViewBag.Torneio = await torneioServico.ObterPorId(TenantContext.TorneioId);
        }

        var fileStorage = HttpContext.RequestServices.GetRequiredService<IFileStorage>();
        ViewBag.StorageBaseUrl = fileStorage.ObterUrlPublica("").TrimEnd('/');

        await next();
    }

    /// <summary>
    /// Salva um arquivo de foto e retorna o caminho relativo, ou null se nenhum arquivo foi enviado.
    /// </summary>
    protected async Task<string?> SalvarFotoAsync(IFormFile? foto, string subpasta)
    {
        if (foto == null || foto.Length == 0) return null;
        var fileStorage = HttpContext.RequestServices.GetRequiredService<IFileStorage>();
        var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
        await using var stream = foto.OpenReadStream();
        return await fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", subpasta);
    }
}
