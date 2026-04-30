using Torneio.Application.Common;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver, TenantContext tenantContext)
    {
        var slug = context.GetRouteValue("slug")?.ToString();

        if (!string.IsNullOrEmpty(slug))
        {
            var torneio = await tenantResolver.ResolverAsync(slug);
            if (torneio is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Torneio não encontrado.");
                return;
            }

            var perfil = context.User.FindFirst("perfil")?.Value;

            // Página pública do torneio (/{slug} sem subpath) é acessível a qualquer usuário logado.
            var isPublicPage = context.Request.Path.Value?.TrimEnd('/').Equals($"/{slug}", StringComparison.OrdinalIgnoreCase) ?? false;

            if (perfil == "AdminGeral")
            {
                tenantContext.DefinirAdminGeral(torneio.Id, torneio.Slug);
            }
            else
            {
                if (!isPublicPage && !string.IsNullOrWhiteSpace(perfil))
                {
                    var torneioIdClaim = context.User.FindFirst("torneio_id")?.Value;
                    if (!string.IsNullOrWhiteSpace(torneioIdClaim) &&
                        (!Guid.TryParse(torneioIdClaim, out var claimTorneioId) || claimTorneioId != torneio.Id))
                    {
                        var slugAtual = context.User.FindFirst("slug")?.Value;
                        var destino = $"/acesso-negado?motivo=torneio-diferente&slugSolicitado={Uri.EscapeDataString(slug)}";
                        if (!string.IsNullOrWhiteSpace(slugAtual))
                            destino += $"&slugAtual={Uri.EscapeDataString(slugAtual)}";

                        context.Response.Redirect(destino);
                        return;
                    }
                }

                tenantContext.DefinirTenant(torneio.Id, torneio.Slug);
            }
        }

        await _next(context);
    }
}
