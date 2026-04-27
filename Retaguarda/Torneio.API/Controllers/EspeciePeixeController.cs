using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.EspeciePeixe;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/especies — catálogo global de espécies de peixe (sem tenant)
/// </summary>
[Route("api/especies")]
public class EspeciePeixeController : BaseController
{
    private readonly IEspeciePeixeServico _servico;
    private readonly IFileStorage _fileStorage;

    public EspeciePeixeController(IEspeciePeixeServico servico, IFileStorage fileStorage)
    {
        _servico = servico;
        _fileStorage = fileStorage;
    }

    /// <summary>Lista todas as espécies — acesso público.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarTodas());

    /// <summary>Obtém uma espécie — acesso público.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Policy = "AdminGeral")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Criar([FromForm] CriarEspeciePeixeFormDto dto)
    {
        var fotoUrl = await SalvarFotoAsync(dto.Foto);
        var criado = await _servico.Criar(new CriarEspeciePeixeDto
        {
            Nome = dto.Nome,
            NomeCientifico = dto.NomeCientifico,
            FotoUrl = fotoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminGeral")]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Atualizar(Guid id, [FromForm] AtualizarEspeciePeixeFormDto dto)
    {
        var atual = await _servico.ObterPorId(id);
        if (atual is null) return NotFound();

        var fotoUrl = await SalvarFotoAsync(dto.Foto) ?? atual.FotoUrl;
        await _servico.Atualizar(id, new AtualizarEspeciePeixeDto
        {
            Nome = dto.Nome,
            NomeCientifico = dto.NomeCientifico,
            FotoUrl = fotoUrl
        });
        return NoContent();
    }

    [Authorize(Policy = "AdminGeral")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }

    private async Task<string?> SalvarFotoAsync(IFormFile? foto)
    {
        if (foto == null || foto.Length == 0) return null;
        var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
        await using var stream = foto.OpenReadStream();
        return await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", "fotos/especies");
    }
}

public class CriarEspeciePeixeFormDto
{
    public string Nome { get; init; } = null!;
    public string? NomeCientifico { get; init; }
    public IFormFile? Foto { get; init; }
}

public class AtualizarEspeciePeixeFormDto
{
    public string Nome { get; init; } = null!;
    public string? NomeCientifico { get; init; }
    public IFormFile? Foto { get; init; }
}
