using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Torneio.Application.DTOs.Patrocinador;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize]
[Route("api/{slug}/patrocinadores")]
public class PatrocinadorController : BaseController
{
    private readonly IPatrocinadorServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly IFileStorage _fileStorage;

    public PatrocinadorController(
        IPatrocinadorServico servico,
        TenantContext tenantContext,
        IFileStorage fileStorage)
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Criar([FromForm] CriarPatrocinadorFormDto dto)
    {
        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/patrocinadores");
        var criado = await _servico.Criar(new CriarPatrocinadorDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            FotoUrl = fotoUrl ?? string.Empty,
            Instagram = dto.Instagram,
            Site = dto.Site,
            Zap = dto.Zap,
            ExibirNaTelaInicial = dto.ExibirNaTelaInicial,
            ExibirNosRelatorios = dto.ExibirNosRelatorios
        });

        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Atualizar(Guid id, [FromForm] AtualizarPatrocinadorFormDto dto)
    {
        var atual = await _servico.ObterPorId(id);
        if (atual is null)
        {
            return NotFound();
        }

        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/patrocinadores") ?? atual.FotoUrl;
        await _servico.Atualizar(id, new AtualizarPatrocinadorDto
        {
            Nome = dto.Nome,
            FotoUrl = fotoUrl,
            Instagram = dto.Instagram,
            Site = dto.Site,
            Zap = dto.Zap,
            ExibirNaTelaInicial = dto.ExibirNaTelaInicial,
            ExibirNosRelatorios = dto.ExibirNosRelatorios
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

public class CriarPatrocinadorFormDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "A imagem e obrigatoria.")]
    public IFormFile Foto { get; init; } = null!;

    public string? Instagram { get; init; }
    public string? Site { get; init; }
    public string? Zap { get; init; }
    public bool ExibirNaTelaInicial { get; init; } = true;
    public bool ExibirNosRelatorios { get; init; } = true;
}

public class AtualizarPatrocinadorFormDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    public IFormFile? Foto { get; init; }
    public string? Instagram { get; init; }
    public string? Site { get; init; }
    public string? Zap { get; init; }
    public bool ExibirNaTelaInicial { get; init; } = true;
    public bool ExibirNosRelatorios { get; init; } = true;
}
