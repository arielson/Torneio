using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Repositories;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize]
[Route("api/{slug}/membros")]
public class MembroController : BaseController
{
    private readonly IMembroServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly IFileStorage _fileStorage;
    private readonly IEquipeRepositorio _equipeRepositorio;

    public MembroController(
        IMembroServico servico,
        TenantContext tenantContext,
        IFileStorage fileStorage,
        IEquipeRepositorio equipeRepositorio)
    {
        _servico = servico;
        _tenantContext = tenantContext;
        _fileStorage = fileStorage;
        _equipeRepositorio = equipeRepositorio;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        if (GetPerfil() == "Fiscal")
        {
            var fiscalId = GetUserId();
            var equipes = await _equipeRepositorio.ListarPorFiscal(_tenantContext.TorneioId, fiscalId);
            var membros = equipes
                .SelectMany(e => e.Membros)
                .GroupBy(m => m.Id)
                .Select(g => new MembroDto
                {
                    Id = g.Key,
                    TorneioId = g.First().TorneioId,
                    Nome = g.First().Nome,
                    FotoUrl = g.First().FotoUrl,
                    TamanhoCamisa = g.First().TamanhoCamisa
                })
                .ToList();

            return Ok(membros);
        }

        return Ok(await _servico.ListarTodos());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Criar([FromBody] CriarMembroDto dto)
    {
        var criado = await _servico.Criar(new CriarMembroDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            FotoUrl = dto.FotoUrl,
            TamanhoCamisa = dto.TamanhoCamisa
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CriarComFoto([FromForm] CriarMembroFormDto dto)
    {
        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/membros");
        var criado = await _servico.Criar(new CriarMembroDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            FotoUrl = fotoUrl,
            TamanhoCamisa = dto.TamanhoCamisa
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarMembroDto dto)
    {
        await _servico.Atualizar(id, dto);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AtualizarComFoto(Guid id, [FromForm] AtualizarMembroFormDto dto)
    {
        var atual = await _servico.ObterPorId(id);
        if (atual is null) return NotFound();

        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/membros") ?? atual.FotoUrl;
        await _servico.Atualizar(id, new AtualizarMembroDto
        {
            Nome = dto.Nome,
            FotoUrl = fotoUrl,
            TamanhoCamisa = dto.TamanhoCamisa
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

public class CriarMembroFormDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;
    public string? TamanhoCamisa { get; init; }
    public IFormFile? Foto { get; init; }
}

public class AtualizarMembroFormDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;
    public string? TamanhoCamisa { get; init; }
    public IFormFile? Foto { get; init; }
}
