using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.Services.Interfaces;
using Torneio.Application.Common;
using Torneio.API.Models;
using Torneio.API.Services;
using System.Security.Claims;

namespace Torneio.API.Controllers;

/// <summary>
/// POST /api/auth/login            → AdminGeral
/// POST /api/{slug}/auth/login     → AdminTorneio ou Fiscal
/// POST /api/{slug}/auth/trocar-senha → usuário autenticado
/// </summary>
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

    [AllowAnonymous]
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

    [AllowAnonymous]
    [HttpPost("api/{slug}/auth/login")]
    public async Task<IActionResult> LoginTenant([FromRoute] string slug, [FromBody] LoginDto dto)
    {
        var torneio = await _tenantResolver.ResolverAsync(slug);
        if (torneio is null) return NotFound(new { erro = $"Torneio '{slug}' não encontrado." });

        UsuarioAutenticadoDto? usuario = dto.Perfil?.Trim().ToLowerInvariant() switch
        {
            "fiscal" =>
                await _autenticacaoServico.AutenticarFiscal(dto.Usuario, dto.Senha, torneio.Id),
            "membro" or "pescador" =>
                await _autenticacaoServico.AutenticarMembro(dto.Usuario, dto.Senha, torneio.Id),
            "admin" or "admintorneio" =>
                await _autenticacaoServico.AutenticarAdminTorneio(dto.Usuario, dto.Senha, torneio.Id),
            _ =>
                await _autenticacaoServico.AutenticarAdminTorneio(dto.Usuario, dto.Senha, torneio.Id)
                ?? await _autenticacaoServico.AutenticarFiscal(dto.Usuario, dto.Senha, torneio.Id)
                ?? await _autenticacaoServico.AutenticarMembro(dto.Usuario, dto.Senha, torneio.Id)
        };

        if (usuario is null) return Unauthorized(new { erro = "Usuário ou senha inválidos." });

        var (token, expiraEm) = _tokenServico.GerarToken(usuario);
        return Ok(new LoginResponseDto
        {
            Token = token,
            Perfil = usuario.Perfil.ToString(),
            Slug = usuario.Slug,
            Nome = usuario.Nome,
            ExpiraEm = expiraEm,
            TrocarSenha = usuario.DeveAlterarSenha
        });
    }

    [HttpPost("api/{slug}/auth/trocar-senha")]
    [Authorize]
    public async Task<IActionResult> TrocarSenha([FromRoute] string slug, [FromBody] TrocarSenhaApiDto dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var perfil    = User.FindFirst("perfil")?.Value ?? "";
        var torneioIdStr = User.FindFirst("torneio_id")?.Value;
        var torneioId = torneioIdStr is not null ? Guid.Parse(torneioIdStr) : (Guid?)null;

        try
        {
            await _autenticacaoServico.TrocarSenha(usuarioId, perfil, dto.SenhaAtual, dto.NovaSenha, torneioId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }

        // Reautentica e devolve novo token sem o claim deve_alterar_senha
        var usuario = torneioId.HasValue
            ? await _autenticacaoServico.AutenticarAdminTorneio(dto.Usuario, dto.NovaSenha, torneioId.Value)
              ?? await _autenticacaoServico.AutenticarFiscal(dto.Usuario, dto.NovaSenha, torneioId.Value)
              ?? await _autenticacaoServico.AutenticarMembro(dto.Usuario, dto.NovaSenha, torneioId.Value)
            : null;

        if (usuario is null)
            return Ok(new { mensagem = "Senha alterada com sucesso." });

        var (token, expiraEm) = _tokenServico.GerarToken(usuario);
        return Ok(new LoginResponseDto
        {
            Token = token,
            Perfil = usuario.Perfil.ToString(),
            Slug = usuario.Slug,
            Nome = usuario.Nome,
            ExpiraEm = expiraEm,
            TrocarSenha = false
        });
    }
}
