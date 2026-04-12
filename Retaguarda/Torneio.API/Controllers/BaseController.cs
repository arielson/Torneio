using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace Torneio.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId() =>
        Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    protected string GetPerfil() =>
        User.FindFirst("perfil")!.Value;

    protected Guid? GetTorneioIdClaim() =>
        Guid.TryParse(User.FindFirst("torneio_id")?.Value, out var id) ? id : null;

    protected string? GetSlugClaim() =>
        User.FindFirst("slug")?.Value;
}
