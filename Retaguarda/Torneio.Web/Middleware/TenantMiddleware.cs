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

            if (perfil == "AdminGeral")
            {
                tenantContext.DefinirAdminGeral(torneio.Id, torneio.Slug);
            }
            else
            {
                // Verify AdminTorneio belongs to this torneio
                if (perfil == "AdminTorneio")
                {
                    var torneioIdClaim = context.User.FindFirst("torneio_id")?.Value;
                    if (torneioIdClaim is null || !Guid.TryParse(torneioIdClaim, out var claimTorneioId) || claimTorneioId != torneio.Id)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }
                }

                tenantContext.DefinirTenant(torneio.Id, torneio.Slug);
            }
        }

        await _next(context);
    }
}
