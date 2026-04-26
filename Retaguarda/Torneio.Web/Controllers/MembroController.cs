using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Membro;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/membros")]
public class MembroController : TorneioBaseController
{
    private readonly IMembroServico _servico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;
    private readonly IPescaProImportacaoServico _pescaPro;
    private readonly IFileStorage _fileStorage;

    public MembroController(TenantContext tenantContext, IMembroServico servico, ITorneioServico torneioServico, ILogAuditoriaServico log, IPescaProImportacaoServico pescaPro, IFileStorage fileStorage)
        : base(tenantContext)
    {
        _servico = servico;
        _torneioServico = torneioServico;
        _log = log;
        _pescaPro = pescaPro;
        _fileStorage = fileStorage;
    }

    private async Task SetTorneioViewBag()
    {
        ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await SetTorneioViewBag();
        var membros = await _servico.ListarTodos();
        return View(membros);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetTorneioViewBag();
        return View(new CriarMembroDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarMembroDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);

        if (torneio is not null && !torneio.ExibirModuloFinanceiro)
        {
            dto = new CriarMembroDto
            {
                TorneioId = dto.TorneioId,
                Nome = dto.Nome,
                FotoUrl = dto.FotoUrl,
                Celular = dto.Celular,
                TamanhoCamisa = null,
                Usuario = dto.Usuario,
                Senha = dto.Senha,
            };
        }

        if (torneio is not null && !torneio.PermitirRegistroPublicoMembro)
        {
            dto = new CriarMembroDto
            {
                TorneioId = dto.TorneioId,
                Nome = dto.Nome,
                FotoUrl = dto.FotoUrl,
                Celular = dto.Celular,
                TamanhoCamisa = dto.TamanhoCamisa,
                Usuario = null,
                Senha = null,
            };
        }

        if (!ModelState.IsValid)
        {
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/membros");
            await _servico.Criar(new CriarMembroDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                FotoUrl = fotoUrl,
                Celular = dto.Celular,
                TamanhoCamisa = dto.TamanhoCamisa,
                Usuario = dto.Usuario,
                Senha = dto.Senha,
            });
            TempData["Sucesso"] = "Membro criado com sucesso.";
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Membros, Acao = "CriarMembro",
                Descricao = $"Pescador criado: {dto.Nome}",
                UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
            });
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var membro = await _servico.ObterPorId(id);
        if (membro is null) return NotFound();
        ViewBag.Membro = membro;
        await SetTorneioViewBag();
        return View(new AtualizarMembroDto
        {
            Nome = membro.Nome,
            FotoUrl = membro.FotoUrl,
            Celular = membro.Celular,
            TamanhoCamisa = membro.TamanhoCamisa
            ,
            Usuario = membro.Usuario
        });
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarMembroDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        var membroAtual = await _servico.ObterPorId(id);
        if (torneio is not null && !torneio.ExibirModuloFinanceiro)
        {
            dto = new AtualizarMembroDto
            {
                Nome = dto.Nome,
                FotoUrl = dto.FotoUrl,
                Celular = dto.Celular,
                TamanhoCamisa = membroAtual?.TamanhoCamisa,
                Usuario = dto.Usuario,
                Senha = dto.Senha,
            };
        }

        if (torneio is not null && !torneio.PermitirRegistroPublicoMembro)
        {
            dto = new AtualizarMembroDto
            {
                Nome = dto.Nome,
                FotoUrl = dto.FotoUrl,
                Celular = dto.Celular,
                TamanhoCamisa = dto.TamanhoCamisa,
                Usuario = null,
                Senha = null,
            };
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Membro = membroAtual;
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/membros") ?? membroAtual?.FotoUrl;
            dto = new AtualizarMembroDto
            {
                Nome = dto.Nome,
                FotoUrl = fotoUrl,
                Celular = dto.Celular,
                TamanhoCamisa = dto.TamanhoCamisa,
                Usuario = dto.Usuario,
                Senha = dto.Senha
            };
            await _servico.Atualizar(id, dto);
            TempData["Sucesso"] = "Membro atualizado.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Membro = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [Authorize(Policy = "AdminGeral")]
    [HttpGet("importar")]
    public async Task<IActionResult> Importar()
    {
        await SetTorneioViewBag();

        if (!_pescaPro.Configurado)
        {
            TempData["Erro"] = "Integracao PescaPro nao configurada. Defina PescaPro:ApiUrl e PescaPro:ApiKey no appsettings.json.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }

        return View();
    }

    [Authorize(Policy = "AdminGeral")]
    [HttpGet("importar/dados")]
    public async Task<IActionResult> ImportarDados()
    {
        try
        {
            var pescadores = await _pescaPro.ListarPescadores();
            var membrosExistentes = (await _servico.ListarTodos())
                .Select(m => m.Nome.Trim().ToLowerInvariant())
                .ToHashSet();

            var resultado = pescadores
                .OrderBy(p => p.Nome, StringComparer.CurrentCultureIgnoreCase)
                .Select(p => new
                {
                    id = p.Id,
                    nome = p.Nome,
                    celular = p.Celular,
                    fotoUrl = p.FotoUrl,
                    jaExiste = membrosExistentes.Contains(p.Nome.Trim().ToLowerInvariant())
                });

            return Json(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }

    [Authorize(Policy = "AdminGeral")]
    [HttpPost("importar")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Importar([FromBody] List<PescadorImportacaoItemDto> pescadores)
    {
        if (pescadores == null || pescadores.Count == 0)
            return BadRequest(new { erro = "Nenhum pescador selecionado." });

        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        var importados = 0;

        foreach (var p in pescadores)
        {
            try
            {
                var fotoLocal = await BaixarFotoExternaAsync(p.FotoUrl, "fotos/membros");
                await _servico.Criar(new CriarMembroDto
                {
                    TorneioId = TenantContext.TorneioId,
                    Nome = p.Nome,
                    Celular = p.Celular,
                    FotoUrl = fotoLocal
                });
                importados++;
            }
            catch
            {
                // ignora duplicatas ou erros individuais e continua importando os demais
            }
        }

        var nomes = string.Join(", ", pescadores.Select(p => p.Nome));
        if (nomes.Length > 900) nomes = nomes[..900] + "...";
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Membros, Acao = "ImportarMembrosPescaPro",
            Descricao = $"{importados}/{pescadores.Count} importados do PescaPro. Nomes: {nomes}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });

        return Ok(new { importados, mensagem = $"{importados} pescador(es) importado(s) com sucesso." });
    }

    [Authorize(Policy = "AdminGeral")]
    [HttpPost("limpar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LimparTodos()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        var (total, fotos) = await _servico.RemoverTodos();
        foreach (var foto in fotos)
            await _fileStorage.RemoverAsync(foto);
        TempData["Sucesso"] = $"{total} pescador(es) removido(s).";
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Membros, Acao = "LimparMembros",
            Descricao = $"Todos os pescadores removidos ({total} total).",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        var membro = await _servico.ObterPorId(id);
        await _servico.Remover(id);
        await RemoverFotoAsync(membro?.FotoUrl);
        TempData["Sucesso"] = "Membro removido.";
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId, NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Membros, Acao = "RemoverMembro",
            Descricao = $"Pescador removido: {membro?.Nome ?? id.ToString()}",
            UsuarioNome = UsuarioNome, UsuarioPerfil = UsuarioPerfil, IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }
}
