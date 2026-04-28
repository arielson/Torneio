using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Torneio.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId()
    {
        var claimValue =
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(claimValue, out var userId))
            throw new UnauthorizedAccessException("Token sem identificador de usuário válido.");

        return userId;
    }

    protected string GetPerfil() =>
        User.FindFirst("perfil")?.Value ?? string.Empty;

    protected Guid? GetTorneioIdClaim() =>
        Guid.TryParse(User.FindFirst("torneio_id")?.Value, out var id) ? id : null;

    protected string? GetSlugClaim() =>
        User.FindFirst("slug")?.Value;
}
