using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Torneio.Application.DTOs.Item;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/itens — AdminTorneio
/// </summary>
[Authorize]
[Route("api/{slug}/itens")]
public class ItemController : BaseController
{
    private readonly IItemServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly IFileStorage _fileStorage;

    public ItemController(IItemServico servico, TenantContext tenantContext, IFileStorage fileStorage)
    {
        _servico = servico;
        _tenantContext = tenantContext;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarPorTorneio(_tenantContext.TorneioId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Criar([FromBody] CriarItemDto dto)
    {
        var criado = await _servico.Criar(new CriarItemDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Comprimento = dto.Comprimento,
            FatorMultiplicador = dto.FatorMultiplicador,
            FotoUrl = dto.FotoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CriarComFoto([FromForm] CriarItemFormDto dto)
    {
        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/itens");
        var criado = await _servico.Criar(new CriarItemDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Comprimento = dto.Comprimento,
            FatorMultiplicador = dto.FatorMultiplicador,
            FotoUrl = fotoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarItemDto dto)
    {
        await _servico.Atualizar(id, dto);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AtualizarComFoto(Guid id, [FromForm] AtualizarItemFormDto dto)
    {
        var atual = await _servico.ObterPorId(id);
        if (atual is null) return NotFound();

        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/itens") ?? atual.FotoUrl;
        await _servico.Atualizar(id, new AtualizarItemDto
        {
            Nome = dto.Nome,
            Comprimento = dto.Comprimento,
            FatorMultiplicador = dto.FatorMultiplicador,
            FotoUrl = fotoUrl
        });
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        return NoContent();
    }

    private async Task<string?> SalvarFotoAsync(IFormFile? foto, string subpasta)
    {
        if (foto == null || foto.Length == 0) return null;
        var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
        await using var stream = foto.OpenReadStream();
        return await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", subpasta);
    }
}

public class CriarItemFormDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    public decimal? Comprimento { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O fator multiplicador deve ser maior que zero.")]
    public decimal FatorMultiplicador { get; init; } = 1.0m;

    public IFormFile? Foto { get; init; }
}

public class AtualizarItemFormDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    public decimal? Comprimento { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O fator multiplicador deve ser maior que zero.")]
    public decimal FatorMultiplicador { get; init; } = 1.0m;

    public IFormFile? Foto { get; init; }
}
