using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize]
[Route("api/{slug}/equipes")]
public class EquipeController : BaseController
{
    private readonly IEquipeServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly IFileStorage _fileStorage;
    private readonly ILogAuditoriaServico _log;
    private readonly ITorneioServico _torneioServico;

    public EquipeController(
        IEquipeServico servico,
        TenantContext tenantContext,
        IFileStorage fileStorage,
        ILogAuditoriaServico log,
        ITorneioServico torneioServico)
    {
        _servico = servico;
        _tenantContext = tenantContext;
        _fileStorage = fileStorage;
        _log = log;
        _torneioServico = torneioServico;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        IEnumerable<EquipeDto> equipes = GetPerfil() == "Fiscal"
            ? (await _servico.ListarTodos()).Where(e => e.FiscalIds.Contains(GetUserId()))
            : await _servico.ListarTodos();

        return Ok(equipes);
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
    public async Task<IActionResult> Criar([FromBody] CriarEquipeDto dto)
    {
        var criado = await _servico.Criar(new CriarEquipeDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Capitao = dto.Capitao,
            QtdVagas = dto.QtdVagas,
            Custo = dto.Custo,
            StatusFinanceiro = dto.StatusFinanceiro,
            FotoUrl = dto.FotoUrl,
            FotoCapitaoUrl = dto.FotoCapitaoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CriarComFotos([FromForm] CriarEquipeFormDto dto)
    {
        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/equipes");
        var fotoCapitaoUrl = await SalvarFotoAsync(dto.FotoCapitao, "fotos/capitaos");
        var criado = await _servico.Criar(new CriarEquipeDto
        {
            TorneioId = _tenantContext.TorneioId,
            Nome = dto.Nome,
            Capitao = dto.Capitao,
            QtdVagas = dto.QtdVagas,
            Custo = dto.Custo,
            StatusFinanceiro = dto.StatusFinanceiro,
            FotoUrl = fotoUrl,
            FotoCapitaoUrl = fotoCapitaoUrl
        });
        return CreatedAtAction(nameof(ObterPorId), new { slug = RouteData.Values["slug"], id = criado.Id }, criado);
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarEquipeDto dto)
    {
        await _servico.Atualizar(id, dto);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AtualizarComFotos(Guid id, [FromForm] AtualizarEquipeFormDto dto)
    {
        var atual = await _servico.ObterPorId(id);
        if (atual is null) return NotFound();

        var fotoUrl = await SalvarFotoAsync(dto.Foto, "fotos/equipes") ?? atual.FotoUrl;
        var fotoCapitaoUrl = await SalvarFotoAsync(dto.FotoCapitao, "fotos/capitaos") ?? atual.FotoCapitaoUrl;

        await _servico.Atualizar(id, new AtualizarEquipeDto
        {
            Nome = dto.Nome,
            Capitao = dto.Capitao,
            QtdVagas = dto.QtdVagas,
            Custo = dto.Custo,
            StatusFinanceiro = dto.StatusFinanceiro,
            FotoUrl = fotoUrl,
            FotoCapitaoUrl = fotoCapitaoUrl
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

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost("{id:guid}/membros/{membroId:guid}")]
    public async Task<IActionResult> AdicionarMembro(Guid id, Guid membroId)
    {
        await _servico.AdicionarMembro(id, membroId);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpDelete("{id:guid}/membros/{membroId:guid}")]
    public async Task<IActionResult> RemoverMembro(Guid id, Guid membroId)
    {
        await _servico.RemoverMembro(id, membroId);
        return NoContent();
    }

    [Authorize(Policy = "AdminTorneio")]
    [HttpPost("reorganizacao-emergencial")]
    public async Task<IActionResult> ReorganizacaoEmergencial([FromBody] ReorganizacaoEmergencialEquipeDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!string.Equals(dto.Confirmacao?.Trim(), "REORGANIZAR", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(dto.Confirmacao), "Digite REORGANIZAR para confirmar a operacao.");
            return ValidationProblem(ModelState);
        }

        var resultado = await _servico.ReorganizarMembroEmergencia(dto.MembroId, dto.EquipeDestinoId);
        var torneio = await _torneioServico.ObterPorId(_tenantContext.TorneioId);

        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = _tenantContext.TorneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Equipes,
            Acao = "ReorganizacaoEmergencialMembroEquipe",
            Descricao = $"REORGANIZACAO EMERGENCIAL | Membro: {resultado.Membro.Nome} | Origem: {resultado.Origem.Nome} | Destino: {resultado.Destino.Nome} | Motivo: {dto.Motivo}",
            UsuarioNome = User.Identity?.Name ?? "App",
            UsuarioPerfil = GetPerfil(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        return Ok(new
        {
            membro = resultado.Membro,
            origem = resultado.Origem,
            destino = resultado.Destino
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

public class CriarEquipeFormDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O capitao e obrigatorio.")]
    public string Capitao { get; init; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Informe ao menos 1 vaga.")]
    public int QtdVagas { get; init; }
    public decimal Custo { get; init; }
    public StatusEmbarcacaoFinanceira StatusFinanceiro { get; init; } = StatusEmbarcacaoFinanceira.Pendente;

    public IFormFile? Foto { get; init; }
    public IFormFile? FotoCapitao { get; init; }
}

public class AtualizarEquipeFormDto
{
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    public string Nome { get; init; } = null!;

    [Required(ErrorMessage = "O capitao e obrigatorio.")]
    public string Capitao { get; init; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Informe ao menos 1 vaga.")]
    public int QtdVagas { get; init; }
    public decimal Custo { get; init; }
    public StatusEmbarcacaoFinanceira StatusFinanceiro { get; init; } = StatusEmbarcacaoFinanceira.Pendente;

    public IFormFile? Foto { get; init; }
    public IFormFile? FotoCapitao { get; init; }
}
