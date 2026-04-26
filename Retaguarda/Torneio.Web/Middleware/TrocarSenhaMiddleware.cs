namespace Torneio.Web.Middleware;

/// <summary>
/// Se o usuário autenticado tem o claim "deve_alterar_senha", redireciona para /trocar-senha
/// em qualquer rota que não seja a própria página de troca ou o logout.
/// </summary>
public class TrocarSenhaMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> _pathsPermitidos =
    [
        "/trocar-senha",
        "/logout"
    ];

    public TrocarSenhaMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.HasClaim("deve_alterar_senha", "true"))
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            var permitido = _pathsPermitidos.Any(p => path.StartsWith(p));

            if (!permitido)
            {
                context.Response.Redirect("/trocar-senha");
                return;
            }
        }

        await _next(context);
    }
}
