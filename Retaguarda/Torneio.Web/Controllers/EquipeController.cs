using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Equipe;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Infrastructure.Services;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/equipes")]
public class EquipeController : TorneioBaseController
{
    private readonly IEquipeServico _servico;
    private readonly IMembroServico _membroServico;
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _log;

    public EquipeController(
        TenantContext tenantContext,
        IEquipeServico servico,
        IMembroServico membroServico,
        ITorneioServico torneioServico,
        ILogAuditoriaServico log) : base(tenantContext)
    {
        _servico = servico;
        _membroServico = membroServico;
        _torneioServico = torneioServico;
        _log = log;
    }

    private async Task SetTorneioViewBag()
    {
        ViewBag.Torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await SetTorneioViewBag();
        var equipes = await _servico.ListarTodos();
        return View(equipes);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Criar()
    {
        await SetTorneioViewBag();
        return View(new CriarEquipeDto { TorneioId = TenantContext.TorneioId });
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEquipeDto dto)
    {
        ModelState.Remove(nameof(dto.TorneioId));

        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is not null && string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
        {
            dto = new CriarEquipeDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = 1,
                FotoUrl = dto.FotoUrl,
                FotoCapitaoUrl = dto.FotoCapitaoUrl,
                Custo = dto.Custo,
                StatusFinanceiro = dto.StatusFinanceiro,
            };
        }

        if (torneio is not null && !torneio.ExibirModuloFinanceiro)
        {
            dto = new CriarEquipeDto
            {
                TorneioId = dto.TorneioId,
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = dto.QtdVagas,
                FotoUrl = dto.FotoUrl,
                FotoCapitaoUrl = dto.FotoCapitaoUrl,
                Custo = 0,
                StatusFinanceiro = StatusEmbarcacaoFinanceira.Pendente,
            };
        }

        if (!ModelState.IsValid)
        {
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/equipes");
            var fotoCapitaoUrl = await SalvarFotoAsync(Request.Form.Files["fotoCapitao"], "fotos/capitaos");

            await _servico.Criar(new CriarEquipeDto
            {
                TorneioId = TenantContext.TorneioId,
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = dto.QtdVagas,
                FotoUrl = fotoUrl,
                FotoCapitaoUrl = fotoCapitaoUrl,
                Custo = dto.Custo,
                StatusFinanceiro = dto.StatusFinanceiro,
            });
            TempData["Sucesso"] = "Equipe criada com sucesso.";
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Equipes,
                Acao = "CriarEquipe",
                Descricao = $"Equipe criada | Nome: {dto.Nome} | Capitao: {dto.Capitao} | Vagas: {dto.QtdVagas} | Custo: {dto.Custo:0.00} | Status financeiro: {dto.StatusFinanceiro}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
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
        var equipe = await _servico.ObterPorId(id);
        if (equipe is null) return NotFound();
        ViewBag.Equipe = equipe;
        await SetTorneioViewBag();

        var dto = new AtualizarEquipeDto
        {
            Nome = equipe.Nome,
            Capitao = equipe.Capitao,
            QtdVagas = equipe.QtdVagas,
            FotoUrl = equipe.FotoUrl,
            FotoCapitaoUrl = equipe.FotoCapitaoUrl,
            Custo = equipe.Custo,
            StatusFinanceiro = equipe.StatusFinanceiro,
        };
        return View(dto);
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, AtualizarEquipeDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        var equipeAtual = await _servico.ObterPorId(id);
        if (torneio is not null && string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
        {
            dto = new AtualizarEquipeDto
            {
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = 1,
                FotoUrl = dto.FotoUrl,
                FotoCapitaoUrl = dto.FotoCapitaoUrl,
                Custo = dto.Custo,
                StatusFinanceiro = dto.StatusFinanceiro,
            };
        }

        if (torneio is not null && !torneio.ExibirModuloFinanceiro)
        {
            dto = new AtualizarEquipeDto
            {
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = dto.QtdVagas,
                FotoUrl = dto.FotoUrl,
                FotoCapitaoUrl = dto.FotoCapitaoUrl,
                Custo = equipeAtual?.Custo ?? 0,
                StatusFinanceiro = equipeAtual?.StatusFinanceiro ?? StatusEmbarcacaoFinanceira.Pendente,
            };
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Equipe = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
        try
        {
            var fotoUrl = await SalvarFotoAsync(Request.Form.Files["foto"], "fotos/equipes") ?? equipeAtual?.FotoUrl;
            var fotoCapitaoUrl = await SalvarFotoAsync(Request.Form.Files["fotoCapitao"], "fotos/capitaos") ?? equipeAtual?.FotoCapitaoUrl;

            dto = new AtualizarEquipeDto
            {
                Nome = dto.Nome,
                Capitao = dto.Capitao,
                QtdVagas = dto.QtdVagas,
                FotoUrl = fotoUrl,
                FotoCapitaoUrl = fotoCapitaoUrl,
                Custo = dto.Custo,
                StatusFinanceiro = dto.StatusFinanceiro,
            };

            await _servico.Atualizar(id, dto);
            TempData["Sucesso"] = "Equipe atualizada.";
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Equipes,
                Acao = "EditarEquipe",
                Descricao = $"Equipe editada | Nome: {dto.Nome} | Capitao: {dto.Capitao} | Vagas: {dto.QtdVagas} | Custo: {dto.Custo:0.00} | Status financeiro: {dto.StatusFinanceiro}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Equipe = await _servico.ObterPorId(id);
            await SetTorneioViewBag();
            return View(dto);
        }
    }

    [HttpPost("{id:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid id)
    {
        var equipe = await _servico.ObterPorId(id);
        await _servico.Remover(id);
        TempData["Sucesso"] = "Equipe removida.";
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Equipes,
            Acao = "RemoverEquipe",
            Descricao = $"Equipe removida | Nome: {equipe?.Nome ?? id.ToString()} | Capitao: {equipe?.Capitao}",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = UsuarioPerfil,
            IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpGet("{id:guid}/membros")]
    public async Task<IActionResult> Membros(Guid id)
    {
        var equipe = await _servico.ObterPorId(id);
        if (equipe is null) return NotFound();
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        ViewBag.Equipe = equipe;
        ViewBag.Torneio = torneio;

        var todosMembros = await _membroServico.ListarTodos();
        return View(todosMembros);
    }

    [HttpPost("{id:guid}/membros/{membroId:guid}/adicionar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarMembro(Guid id, Guid membroId)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        try
        {
            var equipe = await _servico.ObterPorId(id);
            var membro = await _membroServico.ObterPorId(membroId);
            await _servico.AdicionarMembro(id, membroId);
            TempData["Sucesso"] = "Membro adicionado.";
            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio.NomeTorneio,
                Categoria = CategoriaLog.Equipes,
                Acao = "AdicionarMembroEquipe",
                Descricao = $"Membro adicionado a equipe | Equipe: {equipe?.Nome} | Membro: {membro?.Nome}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Membros), new { slug = Slug, id });
    }

    [HttpPost("{id:guid}/membros/{membroId:guid}/remover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverMembro(Guid id, Guid membroId)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        var equipe = await _servico.ObterPorId(id);
        var membro = await _membroServico.ObterPorId(membroId);
        await _servico.RemoverMembro(id, membroId);
        TempData["Sucesso"] = "Membro removido da equipe.";
        await _log.Registrar(new RegistrarLogDto
        {
            TorneioId = TenantContext.TorneioId,
            NomeTorneio = torneio.NomeTorneio,
            Categoria = CategoriaLog.Equipes,
            Acao = "RemoverMembroEquipe",
            Descricao = $"Membro removido da equipe | Equipe: {equipe?.Nome} | Membro: {membro?.Nome}",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = UsuarioPerfil,
            IpAddress = IpAddress
        });
        return RedirectToAction(nameof(Membros), new { slug = Slug, id });
    }

    [HttpGet("reorganizacao-emergencial")]
    public async Task<IActionResult> ReorganizacaoEmergencial()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        ViewBag.Torneio = torneio;
        ViewBag.Equipes = await _servico.ListarTodos();
        ViewBag.Membros = await _membroServico.ListarTodos();
        return View(new ReorganizacaoEmergencialEquipeDto());
    }

    [HttpPost("reorganizacao-emergencial")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorganizacaoEmergencial(ReorganizacaoEmergencialEquipeDto dto)
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        if (string.Equals(torneio.ModoSorteio, nameof(ModoSorteio.Nenhum), StringComparison.Ordinal))
            return RedirectToAction(nameof(Index), new { slug = Slug });

        if (!string.Equals(dto.Confirmacao?.Trim(), "REORGANIZAR", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(dto.Confirmacao), "Digite REORGANIZAR para confirmar a operacao.");

        if (!ModelState.IsValid)
        {
            ViewBag.Torneio = torneio;
            ViewBag.Equipes = await _servico.ListarTodos();
            ViewBag.Membros = await _membroServico.ListarTodos();
            return View(dto);
        }

        try
        {
            var resultado = await _servico.ReorganizarMembroEmergencia(dto.MembroId, dto.EquipeDestinoId);
            TempData["Sucesso"] = "Reorganizacao emergencial registrada com sucesso.";

            await _log.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio.NomeTorneio,
                Categoria = CategoriaLog.Equipes,
                Acao = "ReorganizacaoEmergencialMembroEquipe",
                Descricao = $"REORGANIZACAO EMERGENCIAL | Membro: {resultado.Membro.Nome} | Origem: {resultado.Origem.Nome} | Destino: {resultado.Destino.Nome} | Motivo: {dto.Motivo}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });

            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Torneio = torneio;
            ViewBag.Equipes = await _servico.ListarTodos();
            ViewBag.Membros = await _membroServico.ListarTodos();
            return View(dto);
        }
    }
}
