using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Captura;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.API.Controllers;

[Authorize]
[Route("api/{slug}/capturas")]
public class CapturaController : BaseController
{
    private readonly ICapturaServico _servico;
    private readonly ILogAuditoriaServico _log;
    private readonly ITorneioServico _torneioServico;
    private readonly IFileStorage _fileStorage;

    public CapturaController(
        ICapturaServico servico,
        ILogAuditoriaServico log,
        ITorneioServico torneioServico,
        IFileStorage fileStorage)
    {
        _servico = servico;
        _log = log;
        _torneioServico = torneioServico;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid? equipeId, [FromQuery] Guid? membroId)
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
    [Consumes("application/json")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCapturaDto dto)
    {
        var criado = await _servico.Registrar(dto);
        await RegistrarLogCaptura(dto.TorneioId, criado);
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegistrarComFoto([FromForm] RegistrarCapturaFormDto dto)
    {
        var fotoUrl = await SalvarFotoAsync(dto.Foto, "capturas");
        var criado = await _servico.Registrar(new RegistrarCapturaDto
        {
            TorneioId = dto.TorneioId,
            ItemId = dto.ItemId,
            MembroId = dto.MembroId,
            EquipeId = dto.EquipeId,
            TamanhoMedida = dto.TamanhoMedida,
            FotoUrl = fotoUrl,
            DataHora = dto.DataHora,
            PendenteSync = dto.PendenteSync,
            Origem = OrigemCaptura.App,
            FonteFoto = dto.FonteFoto
        });

        await RegistrarLogCaptura(dto.TorneioId, criado);
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

    private async Task RegistrarLogCaptura(Guid torneioId, CapturaDto criado)
    {
        var torneio = torneioId != Guid.Empty
            ? await _torneioServico.ObterPorId(torneioId)
            : null;

        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId != Guid.Empty ? torneioId : null,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Capturas,
            Acao = "RegistrarCapturaApp",
            Descricao = $"Captura registrada pelo app | Item: {criado.NomeItem} | Pescador: {criado.NomeMembro} | Equipe: {criado.NomeEquipe} | Medida: {criado.TamanhoMedida} | Data: {criado.DataHora:dd/MM/yyyy HH:mm}",
            UsuarioNome = User.Identity?.Name ?? "-",
            UsuarioPerfil = GetPerfil(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }

    private async Task<string?> SalvarFotoAsync(IFormFile? foto, string subpasta)
    {
        if (foto == null || foto.Length == 0) return null;

        var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
        await using var stream = foto.OpenReadStream();
        return await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", subpasta);
    }
}

public class RegistrarCapturaFormDto
{
    public Guid TorneioId { get; init; }
    public Guid ItemId { get; init; }
    public Guid MembroId { get; init; }
    public Guid EquipeId { get; init; }
    public decimal TamanhoMedida { get; init; }
    public DateTime DataHora { get; init; }
    public bool PendenteSync { get; init; }
    public FonteFoto? FonteFoto { get; init; }
    public IFormFile? Foto { get; init; }
}
