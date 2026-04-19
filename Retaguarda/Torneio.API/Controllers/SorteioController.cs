using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.API.Models;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Sorteio;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/sorteio — AdminTorneio
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/sorteio")]
public class SorteioController : BaseController
{
    private readonly ISorteioAppServico _servico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public SorteioController(ISorteioAppServico servico, ITorneioServico torneioServico, ILogAuditoriaServico log)
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
    public async Task<IActionResult> Realizar()
    {
        var resultado = await _servico.RealizarSorteio();
        var torneio = await _torneioServico.ObterPorId(GetTorneioIdClaim() ?? Guid.Empty);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = GetTorneioIdClaim(), NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioCalculado",
            Descricao = $"Sorteio calculado via app ({resultado.Count()} {(torneio?.LabelMembroPlural ?? "membros").ToLower()} distribuídos). Aguardando confirmação.",
            UsuarioNome = UsuarioNome, UsuarioPerfil = GetPerfil(), IpAddress = UsuarioIp,
        });
        return Ok(resultado);
    }

    /// <summary>Confirma e persiste o resultado calculado.</summary>
    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar([FromBody] List<ConfirmarSorteioItemDto> itens)
    {
        await _servico.ConfirmarSorteio(itens);
        var torneio = await _torneioServico.ObterPorId(GetTorneioIdClaim() ?? Guid.Empty);
        var resumo = string.Join(" | ", itens
            .GroupBy(i => i.NomeEquipe)
            .Select(g => $"{g.Key}: {string.Join(", ", g.OrderBy(i => i.Posicao).Select(i => i.NomeMembro))}"));
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = GetTorneioIdClaim(), NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioConfirmado",
            Descricao = $"Sorteio confirmado via app: {itens.Count} {(torneio?.LabelMembroPlural ?? "membros").ToLower()} | {resumo}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = GetPerfil(), IpAddress = UsuarioIp,
        });
        return NoContent();
    }

    [HttpPut("{id:guid}/posicao")]
    public async Task<IActionResult> AjustarPosicao(Guid id, [FromBody] AjustarPosicaoDto dto)
    {
        await _servico.AjustarPosicao(id, dto.Posicao);
        var torneio = await _torneioServico.ObterPorId(GetTorneioIdClaim() ?? Guid.Empty);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = GetTorneioIdClaim(), NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "PosicaoAjustada",
            Descricao = $"Posição do sorteio ajustada para {dto.Posicao} (id: {id}) via app.",
            UsuarioNome = UsuarioNome, UsuarioPerfil = GetPerfil(), IpAddress = UsuarioIp,
        });
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Limpar()
    {
        var torneio = await _torneioServico.ObterPorId(GetTorneioIdClaim() ?? Guid.Empty);
        var resultadoAnterior = await _servico.ObterResultado();
        var resumo = string.Join(" | ", resultadoAnterior
            .GroupBy(r => r.NomeEquipe)
            .Select(g => $"{g.Key}: {string.Join(", ", g.OrderBy(r => r.Posicao).Select(r => r.NomeMembro))}"));
        await _servico.LimparSorteio();
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = GetTorneioIdClaim(), NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Sorteio, Acao = "SorteioLimpo",
            Descricao = $"Resultado do sorteio removido via app | {(string.IsNullOrEmpty(resumo) ? "vazio" : resumo)}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = GetPerfil(), IpAddress = UsuarioIp,
        });
        return NoContent();
    }
}
