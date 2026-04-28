using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Interfaces.Services;
using Torneio.Infrastructure.Services;
using Torneio.Web.Models;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("{slug}/admin")]
public class TorneioAdminController : TorneioBaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _logAuditoriaServico;
    private readonly IFileStorage _fileStorage;

    public TorneioAdminController(
        TenantContext tenantContext,
        ITorneioServico torneioServico,
        ILogAuditoriaServico logAuditoriaServico,
        IFileStorage fileStorage)
        : base(tenantContext)
    {
        _torneioServico = torneioServico;
        _logAuditoriaServico = logAuditoriaServico;
        _fileStorage = fileStorage;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();
        return View(torneio);
    }

    [HttpGet("editar")]
    public async Task<IActionResult> Editar()
    {
        var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneio is null) return NotFound();

        return View(new EditarDadosTorneioAdminViewModel
        {
            NomeTorneio = torneio.NomeTorneio,
            LogoUrl = torneio.LogoUrl,
            Descricao = torneio.Descricao,
            ObservacoesInternas = torneio.ObservacoesInternas,
            QtdGanhadores = torneio.QtdGanhadores,
            UsarFatorMultiplicador = torneio.UsarFatorMultiplicador,
            PermitirCapturaOffline = torneio.PermitirCapturaOffline,
            ExibirModuloFinanceiro = torneio.ExibirModuloFinanceiro,
            ExibirParticipantesPublicos = torneio.ExibirParticipantesPublicos,
            ExibirNaListaInicialPublica = torneio.ExibirNaListaInicialPublica,
            ExibirNaPesquisaPublica = torneio.ExibirNaPesquisaPublica,
            PremiacaoPorEquipe = torneio.PremiacaoPorEquipe,
            PremiacaoPorMembro = torneio.PremiacaoPorMembro,
            ApenasMaiorCapturaPorPescador = torneio.ApenasMaiorCapturaPorPescador,
            CorPrimaria = torneio.CorPrimaria
        });
    }

    [HttpPost("editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarDadosTorneioAdminViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var torneioAtual = await _torneioServico.ObterPorId(TenantContext.TorneioId);
        if (torneioAtual is null) return NotFound();

        try
        {
            var logoUrl = await ResolverLogoAsync(model.LogoArquivo, model.LogoUrl) ?? torneioAtual.LogoUrl;
            var dto = new AtualizarTorneioDto
            {
                NomeTorneio = model.NomeTorneio,
                DataTorneio = torneioAtual.DataTorneio,
                Descricao = model.Descricao,
                ObservacoesInternas = model.ObservacoesInternas,
                LogoUrl = logoUrl,
                LabelEquipe = torneioAtual.LabelEquipe,
                LabelEquipePlural = torneioAtual.LabelEquipePlural,
                LabelMembro = torneioAtual.LabelMembro,
                LabelMembroPlural = torneioAtual.LabelMembroPlural,
                LabelSupervisor = torneioAtual.LabelSupervisor,
                LabelSupervisorPlural = torneioAtual.LabelSupervisorPlural,
                LabelItem = torneioAtual.LabelItem,
                LabelItemPlural = torneioAtual.LabelItemPlural,
                LabelCaptura = torneioAtual.LabelCaptura,
                LabelCapturaPlural = torneioAtual.LabelCapturaPlural,
                UsarFatorMultiplicador = model.UsarFatorMultiplicador,
                MedidaCaptura = torneioAtual.MedidaCaptura,
                PermitirCapturaOffline = model.PermitirCapturaOffline,
                ExibirModuloFinanceiro = model.ExibirModuloFinanceiro,
                PermitirRegistroPublicoMembro = torneioAtual.PermitirRegistroPublicoMembro,
                ExibirParticipantesPublicos = model.ExibirParticipantesPublicos,
                ExibirNaListaInicialPublica = model.ExibirNaListaInicialPublica,
                ExibirNaPesquisaPublica = model.ExibirNaPesquisaPublica,
                ModoSorteio = Enum.Parse<Domain.Enums.ModoSorteio>(torneioAtual.ModoSorteio),
                QtdGanhadores = model.QtdGanhadores,
                PremiacaoPorEquipe = model.PremiacaoPorEquipe,
                PremiacaoPorMembro = model.PremiacaoPorMembro,
                ApenasMaiorCapturaPorPescador = model.ApenasMaiorCapturaPorPescador,
                CorPrimaria = string.IsNullOrWhiteSpace(model.CorPrimaria) ? null : model.CorPrimaria.Trim()
            };

            await _torneioServico.Atualizar(TenantContext.TorneioId, dto);
            var alteracoes = DescreverAlteracoes(torneioAtual, model, logoUrl);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneioAtual.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "EditarDadosTorneio",
                Descricao = $"Dados do torneio atualizados via retaguarda web. Alteracoes: {alteracoes}",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = "Dados do torneio atualizados com sucesso.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost("liberar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liberar()
    {
        try
        {
            await _torneioServico.Liberar(TenantContext.TorneioId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "LiberarTorneio",
                Descricao = "Status do torneio alterado para Liberado via retaguarda web.",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = "Torneio liberado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("finalizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar()
    {
        try
        {
            await _torneioServico.Finalizar(TenantContext.TorneioId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "FinalizarTorneio",
                Descricao = "Status do torneio alterado para Finalizado via retaguarda web.",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = "Torneio finalizado.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpPost("reabrir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reabrir()
    {
        try
        {
            await _torneioServico.Reabrir(TenantContext.TorneioId);
            var torneio = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = torneio?.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "ReabrirTorneio",
                Descricao = "Status do torneio alterado para Aberto via retaguarda web.",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = "Torneio reaberto.";
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { slug = Slug });
    }

    [HttpGet("clonar")]
    public IActionResult Clonar() => View();

    [HttpPost("clonar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clonar(string novoSlug, string novoNome)
    {
        try
        {
            var novo = await _torneioServico.ClonarTorneio(TenantContext.TorneioId, novoSlug, novoNome);
            var origem = await _torneioServico.ObterPorId(TenantContext.TorneioId);
            await _logAuditoriaServico.Registrar(new RegistrarLogDto
            {
                TorneioId = TenantContext.TorneioId,
                NomeTorneio = origem?.NomeTorneio,
                Categoria = CategoriaLog.Torneios,
                Acao = "ClonarTorneio",
                Descricao = $"Torneio clonado via retaguarda web | Origem: {origem?.NomeTorneio} ({Slug}) | Novo: {novo.NomeTorneio} ({novo.Slug})",
                UsuarioNome = UsuarioNome,
                UsuarioPerfil = UsuarioPerfil,
                IpAddress = IpAddress
            });
            TempData["Sucesso"] = $"Torneio \"{novo.NomeTorneio}\" criado a partir desta edição.";
            return RedirectToAction(nameof(Index), new { slug = Slug });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            return View();
        }
    }

    private async Task<string?> ResolverLogoAsync(IFormFile? arquivo, string? urlTexto)
    {
        if (arquivo != null && arquivo.Length > 0)
        {
            var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            await using var stream = arquivo.OpenReadStream();
            var path = await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", "logos");
            return _fileStorage.ObterUrlPublica(path);
        }

        return string.IsNullOrWhiteSpace(urlTexto) ? null : urlTexto.Trim();
    }

    private static string DescreverAlteracoes(TorneioDto atual, EditarDadosTorneioAdminViewModel novo, string? logoUrl)
    {
        var alteracoes = new List<string>();

        Registrar(alteracoes, "Nome", atual.NomeTorneio, novo.NomeTorneio);
        Registrar(alteracoes, "Descricao", atual.Descricao, novo.Descricao);
        Registrar(alteracoes, "Observacoes internas", atual.ObservacoesInternas, novo.ObservacoesInternas);
        Registrar(alteracoes, "Quantidade de ganhadores", atual.QtdGanhadores.ToString(), novo.QtdGanhadores.ToString());
        Registrar(alteracoes, "Usar fator multiplicador", atual.UsarFatorMultiplicador ? "Sim" : "Nao", novo.UsarFatorMultiplicador ? "Sim" : "Nao");
        Registrar(alteracoes, "Permitir captura offline", atual.PermitirCapturaOffline ? "Sim" : "Nao", novo.PermitirCapturaOffline ? "Sim" : "Nao");
        Registrar(alteracoes, "Exibir modulo financeiro", atual.ExibirModuloFinanceiro ? "Sim" : "Nao", novo.ExibirModuloFinanceiro ? "Sim" : "Nao");
        Registrar(alteracoes, "Exibir participantes publicos", atual.ExibirParticipantesPublicos ? "Sim" : "Nao", novo.ExibirParticipantesPublicos ? "Sim" : "Nao");
        Registrar(alteracoes, "Exibir na lista inicial publica", atual.ExibirNaListaInicialPublica ? "Sim" : "Nao", novo.ExibirNaListaInicialPublica ? "Sim" : "Nao");
        Registrar(alteracoes, "Exibir na pesquisa publica", atual.ExibirNaPesquisaPublica ? "Sim" : "Nao", novo.ExibirNaPesquisaPublica ? "Sim" : "Nao");
        Registrar(alteracoes, "Premiacao por equipe", atual.PremiacaoPorEquipe ? "Sim" : "Nao", novo.PremiacaoPorEquipe ? "Sim" : "Nao");
        Registrar(alteracoes, "Premiacao por membro", atual.PremiacaoPorMembro ? "Sim" : "Nao", novo.PremiacaoPorMembro ? "Sim" : "Nao");
        Registrar(alteracoes, "Apenas maior captura por pescador", atual.ApenasMaiorCapturaPorPescador ? "Sim" : "Nao", novo.ApenasMaiorCapturaPorPescador ? "Sim" : "Nao");
        Registrar(alteracoes, "Cor primaria", atual.CorPrimaria, novo.CorPrimaria);
        Registrar(alteracoes, "Logo", atual.LogoUrl, logoUrl);

        return alteracoes.Count == 0 ? "nenhuma alteracao relevante detectada" : string.Join("; ", alteracoes);
    }

    private static void Registrar(List<string> alteracoes, string campo, string? anterior, string? atual)
    {
        var antes = (anterior ?? string.Empty).Trim();
        var depois = (atual ?? string.Empty).Trim();
        if (!string.Equals(antes, depois, StringComparison.Ordinal))
            alteracoes.Add($"{campo}: '{antes}' -> '{depois}'");
    }
}
