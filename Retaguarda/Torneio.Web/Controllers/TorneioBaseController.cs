using System.Security.Claims;
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

    protected string UsuarioNome => User.Identity?.Name ?? "—";
    protected string UsuarioPerfil => User.FindFirst("perfil")?.Value ?? "AdminTorneio";
    protected string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

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

    /// <summary>
    /// Remove o arquivo local de uma foto. Ignora URLs externas (http) e valores nulos.
    /// </summary>
    protected async Task RemoverFotoAsync(string? fotoUrl)
    {
        if (string.IsNullOrWhiteSpace(fotoUrl)) return;
        if (fotoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;
        var fileStorage = HttpContext.RequestServices.GetRequiredService<IFileStorage>();
        await fileStorage.RemoverAsync(fotoUrl);
    }

    /// <summary>
    /// Baixa uma imagem de uma URL externa e salva localmente. Retorna o caminho relativo ou null em caso de falha.
    /// </summary>
    protected async Task<string?> BaixarFotoExternaAsync(string? urlExterna, string subpasta)
    {
        if (string.IsNullOrWhiteSpace(urlExterna) || !urlExterna.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return null;
        try
        {
            var http = HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
            var bytes = await http.GetByteArrayAsync(urlExterna);
            var ext = Path.GetExtension(new Uri(urlExterna).AbsolutePath).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) || ext.Length > 5) ext = ".jpg";
            var fileStorage = HttpContext.RequestServices.GetRequiredService<IFileStorage>();
            using var ms = new MemoryStream(bytes);
            return await fileStorage.SalvarAsync(ms, $"{Guid.NewGuid()}{ext}", subpasta);
        }
        catch
        {
            return null; // foto é opcional — não bloqueia a operação
        }
    }
}
