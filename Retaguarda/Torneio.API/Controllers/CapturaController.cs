using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/capturas — Fiscal e AdminTorneio
/// </summary>
[Authorize]
[Route("api/{slug}/capturas")]
public class CapturaController : BaseController
{
    private readonly ICapturaServico _servico;
    private readonly ILogAuditoriaServico _log;
    private readonly ITorneioServico _torneioServico;

    public CapturaController(ICapturaServico servico, ILogAuditoriaServico log, ITorneioServico torneioServico)
    {
        _servico = servico;
        _log = log;
        _torneioServico = torneioServico;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? equipeId,
        [FromQuery] Guid? membroId)
    {
        if (equipeId.HasValue)
            return Ok(await _servico.ListarPorEquipe(equipeId.Value));

        if (membroId.HasValue)
            return Ok(await _servico.ListarPorMembro(membroId.Value));

        return Ok(await _servico.ListarTodos());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCapturaDto dto)
    {
        var criado = await _servico.Registrar(dto);
        var torneio = dto.TorneioId != Guid.Empty
            ? await _torneioServico.ObterPorId(dto.TorneioId)
            : null;
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = dto.TorneioId != Guid.Empty ? dto.TorneioId : null,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Capturas, Acao = "RegistrarCapturaApp",
            Descricao = $"Captura registrada pelo app | Item: {criado.NomeItem} | Pescador: {criado.NomeMembro} | Equipe: {criado.NomeEquipe} | Medida: {criado.TamanhoMedida} | Data: {criado.DataHora:dd/MM/yyyy HH:mm}",
            UsuarioNome = User.Identity?.Name ?? "—",
            UsuarioPerfil = GetPerfil(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}/tamanho")]
    public async Task<IActionResult> AlterarTamanho(Guid id, [FromBody] AlterarTamanhoCapturaDto dto)
    {
        var capturaAntes = await _servico.ObterPorId(id);
        if (capturaAntes is null)
            return NotFound(new { erro = "Captura nao encontrada." });

        try
        {
            await _servico.AlterarTamanho(id, dto.TamanhoMedida);
            var capturaDepois = await _servico.ObterPorId(id);
            var torneio = capturaAntes.TorneioId != Guid.Empty
                ? await _torneioServico.ObterPorId(capturaAntes.TorneioId)
                : null;

            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = capturaAntes.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Capturas,
                Acao = "AlterarTamanhoCapturaApp",
                Descricao = $"Tamanho da captura alterado pelo app | Item: {capturaAntes.NomeItem} | Pescador: {capturaAntes.NomeMembro} | Equipe: {capturaAntes.NomeEquipe} | Medida anterior: {capturaAntes.TamanhoMedida} | Nova medida: {capturaDepois?.TamanhoMedida ?? dto.TamanhoMedida}",
                UsuarioNome = User.Identity?.Name ?? "-",
                UsuarioPerfil = GetPerfil(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            return Ok(capturaDepois);
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }
}
