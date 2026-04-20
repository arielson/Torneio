using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/admin")]
public class TorneioAdminController : BaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _logAuditoriaServico;

    public TorneioAdminController(
        ITorneioServico torneioServico,
        ILogAuditoriaServico logAuditoriaServico)
    {
        _torneioServico = torneioServico;
        _logAuditoriaServico = logAuditoriaServico;
    }

    private string UsuarioNome => User.Identity?.Name ?? "—";
    private string? UsuarioIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("liberar")]
    public async Task<IActionResult> Liberar()
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        await _torneioServico.Liberar(torneioId);
        var torneio = await _torneioServico.ObterPorId(torneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Torneios,
            Acao = "LiberarTorneio",
            Descricao = "Status do torneio alterado para Liberado via app.",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = GetPerfil(),
            IpAddress = UsuarioIp
        });
        return NoContent();
    }

    [HttpPost("finalizar")]
    public async Task<IActionResult> Finalizar()
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        await _torneioServico.Finalizar(torneioId);
        var torneio = await _torneioServico.ObterPorId(torneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Torneios,
            Acao = "FinalizarTorneio",
            Descricao = "Status do torneio alterado para Finalizado via app.",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = GetPerfil(),
            IpAddress = UsuarioIp
        });
        return NoContent();
    }

    [HttpPost("reabrir")]
    public async Task<IActionResult> Reabrir()
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        await _torneioServico.Reabrir(torneioId);
        var torneio = await _torneioServico.ObterPorId(torneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Torneios,
            Acao = "ReabrirTorneio",
            Descricao = "Status do torneio alterado para Aberto via app.",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = GetPerfil(),
            IpAddress = UsuarioIp
        });
        return NoContent();
    }
}
