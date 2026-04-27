using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Torneio.Application.DTOs.EspeciePeixe;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminGeral")]
[Route("admin/especies")]
public class EspeciePeixeController : Controller
{
    private readonly IEspeciePeixeServico _servico;
    private readonly IFileStorage _fileStorage;

    public EspeciePeixeController(IEspeciePeixeServico servico, IFileStorage fileStorage)
    {
        _servico = servico;
        _fileStorage = fileStorage;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ViewBag.StorageBaseUrl = _fileStorage.ObterUrlPublica("").TrimEnd('/');
        await next();
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var lista = await _servico.ListarTodas();
        return View(lista);
    }

    [HttpGet("criar")]
    public IActionResult Criar() => View(new CriarEspeciePeixeDto());

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEspeciePeixeDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"]);
            await _servico.Criar(new CriarEspeciePeixeDto
            {
                Nome = dto.Nome,
                NomeCientifico = dto.NomeCientifico,
                FotoUrl = fotoUrl,
            });
            TempData["Sucesso"] = "Espécie criada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var especie = await _servico.ObterPorId(id);
        if (especie is null) return NotFound();
        ViewBag.Especie = especie;
        return View(new AtualizarEspeciePeixeDto
        {
            Nome = especie.Nome,
            NomeCientifico = especie.NomeCientifico,
            FotoUrl = especie.FotoUrl,
        });
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarEspeciePeixeDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Especie = await _servico.ObterPorId(id);
            return View(dto);
        }
        try
        {
            var especieAtual = await _servico.ObterPorId(id);
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"]) ?? especieAtual?.FotoUrl;
            await _servico.Atualizar(id, new AtualizarEspeciePeixeDto
            {
                Nome = dto.Nome,
                NomeCientifico = dto.NomeCientifico,
                FotoUrl = fotoUrl,
            });
            TempData["Sucesso"] = "Espécie atualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Especie = await _servico.ObterPorId(id);
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        try
        {
            await _servico.Remover(id);
            TempData["Sucesso"] = "Espécie removida.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SalvarFotoAsync(IFormFile? foto)
    {
        if (foto == null || foto.Length == 0) return null;
        var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
        await using var stream = foto.OpenReadStream();
        return await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", "fotos/especies");
    }
}
