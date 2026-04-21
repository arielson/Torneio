using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Patrocinador;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/patrocinadores")]
public class PatrocinadorController : TorneioBaseController
{
    private readonly IPatrocinadorServico _servico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public PatrocinadorController(
        TenantContext tenantContext,
        IPatrocinadorServico servico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log) : base(tenantContext)
    {
        _servico = servico;
        _torneioServico = torneioServico;
        _log = log;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var patrocinadores = await _servico.ListarPorTorneio(TenantContext.TorneioId);
        return View(patrocinadores);
    }

    [HttpGet("criar")]
    public IActionResult Criar() => View(new CriarPatrocinadorDto { TorneioId = TenantContext.TorneioId });

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarPatrocinadorDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));
        ModelState.Remove(nameof(dto.FotoUrl));

        var foto = Request.Form.Files["foto"];
        if (foto == null || foto.Length == 0)
        {
            ModelState.AddModelError(nameof(dto.FotoUrl), "A imagem e obrigatoria.");
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var fotoUrl = await SalvarFotoAsync(foto, "fotos/patrocinadores");
            var criado = await _servico.Criar(new CriarPatrocinadorDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                FotoUrl = fotoUrl!,
                Instagram = dto.Instagram,
                Site = dto.Site,
                Zap = dto.Zap,
                ExibirNaTelaInicial = dto.ExibirNaTelaInicial,
                ExibirNosRelatorios = dto.ExibirNosRelatorios
            });

            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Patrocinadores,
                Acao = "CriarPatrocinador",
                Descricao = $"Patrocinador criado | Nome: {criado.Nome} | Instagram: {criado.Instagram ?? "-"} | Site: {criado.Site ?? "-"} | Zap: {criado.Zap ?? "-"}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });

            TempData["Sucesso"] = "Patrocinador criado com sucesso.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
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
        var patrocinador = await _servico.ObterPorId(id);
        if (patrocinador is null) return NotFound();

        ViewBag.Patrocinador = patrocinador;
        return View(new AtualizarPatrocinadorDto
        {
            Nome = patrocinador.Nome,
            FotoUrl = patrocinador.FotoUrl,
            Instagram = patrocinador.Instagram,
            Site = patrocinador.Site,
            Zap = patrocinador.Zap
            ,
            ExibirNaTelaInicial = patrocinador.ExibirNaTelaInicial,
            ExibirNosRelatorios = patrocinador.ExibirNosRelatorios
        });
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarPatrocinadorDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Patrocinador = await _servico.ObterPorId(id);
            return View(dto);
        }

        try
        {
            var atual = await _servico.ObterPorId(id);
            if (atual is null) return NotFound();

            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/patrocinadores") ?? atual.FotoUrl;
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

            TempData["Sucesso"] = "Patrocinador atualizado.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Patrocinador = await _servico.ObterPorId(id);
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        var patrocinador = await _servico.ObterPorId(id);
        await _servico.Remover(id);

        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Patrocinadores,
            Acao = "RemoverPatrocinador",
            Descricao = $"Patrocinador removido | Nome: {patrocinador?.Nome ?? id.ToString()}",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = UsuarioPerfil,
            IpAddress = IpAddress
        });

        TempData["Sucesso"] = "Patrocinador removido.";
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
