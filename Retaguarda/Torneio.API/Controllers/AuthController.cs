using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.Services.Interfaces;
using Torneio.Application.Common;
using Torneio.API.Models;
using Torneio.API.Services;

namespace Torneio.API.Controllers;

/// <summary>
/// POST /api/auth/login            → AdminGeral
/// POST /api/{slug}/auth/login     → AdminTorneio ou Fiscal
/// </summary>
[AllowAnonymous]
public class AuthController : BaseController
{
    private readonly IAutenticacaoServico _autenticacaoServico;
    private readonly ITenantResolver _tenantResolver;
    private readonly TokenServico _tokenServico;

    public AuthController(
        IAutenticacaoServico autenticacaoServico,
        ITenantResolver tenantResolver,
        TokenServico tokenServico)
    {
        _autenticacaoServico = autenticacaoServico;
        _tenantResolver = tenantResolver;
        _tokenServico = tokenServico;
    }

    [HttpPost("api/auth/login")]
    public async Task<IActionResult> LoginAdminGeral([FromBody] LoginDto dto)
    {
        var usuario = await _autenticacaoServico.AutenticarAdminGeral(dto.Usuario, dto.Senha);
        if (usuario is null) return Unauthorized(new { erro = "Usuário ou senha inválidos." });

        var (token, expiraEm) = _tokenServico.GerarToken(usuario);
        return Ok(new LoginResponseDto
        {
            Token = token,
            Perfil = usuario.Perfil.ToString(),
            Nome = usuario.Nome,
            ExpiraEm = expiraEm
        });
    }

    [HttpPost("api/{slug}/auth/login")]
    public async Task<IActionResult> LoginTenant([FromRoute] string slug, [FromBody] LoginDto dto)
    {
        var torneio = await _tenantResolver.ResolverAsync(slug);
        if (torneio is null) return NotFound(new { erro = $"Torneio '{slug}' não encontrado." });

        UsuarioAutenticadoDto? usuario = dto.Perfil?.Trim().ToLowerInvariant() switch
        {
            "fiscal" =>
                await _autenticacaoServico.AutenticarFiscal(dto.Usuario, dto.Senha, torneio.Id),
            "admin" or "admintorneio" =>
                await _autenticacaoServico.AutenticarAdminTorneio(dto.Usuario, dto.Senha, torneio.Id),
            _ =>
                await _autenticacaoServico.AutenticarAdminTorneio(dto.Usuario, dto.Senha, torneio.Id)
                ?? await _autenticacaoServico.AutenticarFiscal(dto.Usuario, dto.Senha, torneio.Id)
        };

        if (usuario is null) return Unauthorized(new { erro = "Usuário ou senha inválidos." });

        var (token, expiraEm) = _tokenServico.GerarToken(usuario);
        return Ok(new LoginResponseDto
        {
            Token = token,
            Perfil = usuario.Perfil.ToString(),
            Slug = usuario.Slug,
            Nome = usuario.Nome,
            ExpiraEm = expiraEm
        });
    }
}
