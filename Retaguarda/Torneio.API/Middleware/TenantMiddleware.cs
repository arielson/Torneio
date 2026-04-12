using Torneio.Application.Common;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        TenantContext tenantContext,
        ITenantResolver tenantResolver)
    {
        var slug = context.GetRouteValue("slug") as string;

        if (!string.IsNullOrEmpty(slug))
        {
            var torneio = await tenantResolver.ResolverAsync(slug);

            if (torneio is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { erro = $"Torneio '{slug}' não encontrado." });
                return;
            }

            var perfil = context.User.FindFirst("perfil")?.Value;

            if (perfil == "AdminGeral")
                tenantContext.DefinirAdminGeral(torneio.Id, torneio.Slug);
            else
                tenantContext.DefinirTenant(torneio.Id, torneio.Slug);
        }
        else
        {
            // Rotas de plataforma sem slug (ex: /api/admin/torneiros)
            var perfil = context.User.FindFirst("perfil")?.Value;
            if (perfil == "AdminGeral")
                tenantContext.DefinirAdminGeral();
        }

        await _next(context);
    }
}
