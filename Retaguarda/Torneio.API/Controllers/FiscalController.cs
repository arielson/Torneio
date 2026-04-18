using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Torneio.Application.DTOs.Auth;
using Torneio.Application.DTOs.Fiscal;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

/// <summary>
/// /api/{slug}/fiscais — AdminTorneio
/// </summary>
[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/fiscais")]
public class FiscalController : BaseController
{
    private readonly IFiscalServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly IFileStorage _fileStorage;

    public FiscalController(
        IFiscalServico servico,
        TenantContext tenantContext,
        IFileStorage fileStorage)
    {
        _servico = servico;
        _tenantContext = tenantContext;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarTodos());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var dto = await _servico.ObterPorId(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Criar([FromBody] CriarFiscalDto dto)
    {
        var criado = await _servico.Criar(new CriarFiscalDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Usuario = dto.Usuario,
            Senha = dto.Senha,
            FotoUrl = dto.FotoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CriarComFoto([FromForm] CriarFiscalFormDto dto)
    {
        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/fiscais");
        var criado = await _servico.Criar(new CriarFiscalDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Usuario = dto.Usuario,
            Senha = dto.Senha,
            FotoUrl = fotoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarFiscalDto dto)
    {
        await _servico.Atualizar(id, dto);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AtualizarComFoto(Guid id, [FromForm] AtualizarFiscalFormDto dto)
    {
        var atual = await _servico.ObterPorId(id);
        if (atual is null) return NotFound();

        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/fiscais") ?? atual.FotoUrl;
        await _servico.Atualizar(id, new AtualizarFiscalDto
        {
            Nome = dto.Nome,
            Usuario = dto.Usuario,
            Senha = dto.Senha,
            FotoUrl = fotoUrl
        });
        return NoContent();
    }

    [HttpPut("{id:guid}/senha")]
    public async Task<IActionResult> AtualizarSenha(Guid id, [FromBody] AtualizarSenhaDto dto)
    {
        await _servico.AtualizarSenha(id, dto);
        return NoContent();
    }

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

public class CriarFiscalFormDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O usuário é obrigatório.")]
    public string Usuario { get; init; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; init; } = null!;

    public IFormFile? Foto { get; init; }
}

public class AtualizarFiscalFormDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O usuário é obrigatório.")]
    public string Usuario { get; init; } = null!;

    public string? Senha { get; init; }

    public IFormFile? Foto { get; init; }
}
