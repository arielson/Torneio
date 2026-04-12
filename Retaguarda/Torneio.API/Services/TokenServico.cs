using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Torneio.Application.DTOs.Auth;

namespace Torneio.API.Services;

public class TokenServico
{
    private readonly JwtOptions _options;

    public TokenServico(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string token, DateTime expiraEm) GerarToken(UsuarioAutenticadoDto usuario)
    {
        var expiraEm = DateTime.UtcNow.AddHours(_options.ExpiracaoHoras);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, usuario.Nome),
            new("perfil", usuario.Perfil.ToString())
        };

        if (usuario.TorneioId.HasValue)
            claims.Add(new("torneio_id", usuario.TorneioId.Value.ToString()));

        if (!string.IsNullOrEmpty(usuario.Slug))
            claims.Add(new("slug", usuario.Slug));

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
