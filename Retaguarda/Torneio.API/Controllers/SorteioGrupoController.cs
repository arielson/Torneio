using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Sorteio;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/sorteio-grupo — AdminTorneio
/// Sorteio no modo GrupoEquipe: grupos pre-formados sorteiam embarcações.
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/sorteio-grupo")]
public class SorteioGrupoController : BaseController
{
    private readonly ISorteioGrupoAppServico _servico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public SorteioGrupoController(
        ISorteioGrupoAppServico servico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log)
    {
        _servico = servico;
        _torneioServico = torneioServico;
        _log = log;
    }

    private string UsuarioNome => User.Identity?.Name ?? "—";
    private string? UsuarioIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet("pre-condicoes")]
    public async Task<IActionResult> PreCondicoes() =>
        Ok(await _servico.VerificarPreCondicoes());

    [HttpGet]
    public async Task<IActionResult> ObterResultado() =>
        Ok(await _servico.ObterResultado());

    /// <summary>Calcula o sorteio em memória, sem salvar.</summary>
    [HttpPost]
    public async Task<IActionResult> Realizar([FromBody] RealizarSorteioGrupoDto? filtro = null)
    {
        var resultado = (await _servico.RealizarSorteio(filtro)).ToList();
        var torneio = await _torneioServico.ObterPorId(GetTorneioIdClaim() ?? Guid.Empty);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = GetTorneioIdClaim(), NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioGrupoCalculado",
            Descricao = $"Sorteio de grupos calculado via app ({resultado.Count} grupos). Aguardando confirmação.",
            UsuarioNome = UsuarioNome, UsuarioPerfil = GetPerfil(), IpAddress = UsuarioIp,
        });
        return Ok(resultado);
    }

    /// <summary>Confirma e persiste o resultado calculado.</summary>
    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar([FromBody] List<ConfirmarSorteioGrupoItemDto> itens)
    {
        await _servico.ConfirmarSorteio(itens);
        var torneio = await _torneioServico.ObterPorId(GetTorneioIdClaim() ?? Guid.Empty);
        var resumo = string.Join(" | ", itens.Select(i => $"{i.NomeGrupo} → {i.NomeEquipe}"));
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = GetTorneioIdClaim(), NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioGrupoConfirmado",
            Descricao = $"Sorteio de grupos confirmado via app: {itens.Count} grupos | {resumo}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = GetPerfil(), IpAddress = UsuarioIp,
        });
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Limpar()
    {
        var torneio = await _torneioServico.ObterPorId(GetTorneioIdClaim() ?? Guid.Empty);
        var resultado = (await _servico.ObterResultado()).ToList();
        var resumo = string.Join(" | ", resultado.Select(r => $"{r.NomeGrupo} → {r.NomeEquipe}"));
        await _servico.LimparSorteio();
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = GetTorneioIdClaim(), NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioGrupoLimpo",
            Descricao = $"Sorteio de grupos removido via app | {(string.IsNullOrEmpty(resumo) ? "vazio" : resumo)}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = GetPerfil(), IpAddress = UsuarioIp,
        });
        return NoContent();
    }
}
