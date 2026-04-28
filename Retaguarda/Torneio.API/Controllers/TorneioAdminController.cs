using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Torneio.Application.DTOs.Log;
using Torneio.Application.DTOs.Torneio;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.API.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/admin")]
public class TorneioAdminController : BaseController
{
    private readonly ITorneioServico _torneioServico;
    private readonly ILogAuditoriaServico _logAuditoriaServico;
    private readonly IFileStorage _fileStorage;

    public TorneioAdminController(
        ITorneioServico torneioServico,
        ILogAuditoriaServico logAuditoriaServico,
        IFileStorage fileStorage)
    {
        _torneioServico = torneioServico;
        _logAuditoriaServico = logAuditoriaServico;
        _fileStorage = fileStorage;
    }

    private string UsuarioNome => User.Identity?.Name ?? "-";
    private string? UsuarioIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet("configuracao")]
    public async Task<IActionResult> ObterConfiguracao()
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        var torneio = await _torneioServico.ObterPorId(torneioId);
        if (torneio is null) return NotFound();

        return Ok(new TorneioAdminConfiguracaoDto
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

    [HttpPut("configuracao")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AtualizarConfiguracao([FromForm] AtualizarTorneioAdminFormDto dto)
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        var torneioAtual = await _torneioServico.ObterPorId(torneioId);
        if (torneioAtual is null) return NotFound();

        var logoUrl = await SalvarLogoAsync(dto.Logo) ?? torneioAtual.LogoUrl;
        await _torneioServico.Atualizar(torneioId, new AtualizarTorneioDto
        {
            NomeTorneio = dto.NomeTorneio,
            DataTorneio = torneioAtual.DataTorneio,
            Descricao = dto.Descricao,
            ObservacoesInternas = dto.ObservacoesInternas,
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
            UsarFatorMultiplicador = dto.UsarFatorMultiplicador,
            MedidaCaptura = torneioAtual.MedidaCaptura,
            PermitirCapturaOffline = dto.PermitirCapturaOffline,
            ExibirModuloFinanceiro = dto.ExibirModuloFinanceiro,
            PermitirRegistroPublicoMembro = torneioAtual.PermitirRegistroPublicoMembro,
            ExibirParticipantesPublicos = dto.ExibirParticipantesPublicos,
            ExibirNaListaInicialPublica = dto.ExibirNaListaInicialPublica,
            ExibirNaPesquisaPublica = dto.ExibirNaPesquisaPublica,
            ModoSorteio = Enum.Parse<ModoSorteio>(torneioAtual.ModoSorteio),
            QtdGanhadores = dto.QtdGanhadores,
            PremiacaoPorEquipe = dto.PremiacaoPorEquipe,
            PremiacaoPorMembro = dto.PremiacaoPorMembro,
            ApenasMaiorCapturaPorPescador = dto.ApenasMaiorCapturaPorPescador,
            CorPrimaria = string.IsNullOrWhiteSpace(dto.CorPrimaria) ? null : dto.CorPrimaria.Trim()
        });

        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneioAtual.NomeTorneio,
            Categoria = CategoriaLog.Torneios,
            Acao = "EditarDadosTorneio",
            Descricao = $"Dados do torneio atualizados via app. Alteracoes: {DescreverAlteracoes(torneioAtual, dto, logoUrl)}",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = GetPerfil(),
            IpAddress = UsuarioIp
        });

        return NoContent();
    }

    [HttpPost("liberar")]
    public async Task<IActionResult> Liberar()
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        await _torneioServico.Liberar(torneioId);
        var torneio = await _torneioServico.ObterPorId(torneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Torneios,
            Acao = "LiberarTorneio",
            Descricao = "Status do torneio alterado para Liberado via app.",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = GetPerfil(),
            IpAddress = UsuarioIp
        });
        return NoContent();
    }

    [HttpPost("finalizar")]
    public async Task<IActionResult> Finalizar()
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        await _torneioServico.Finalizar(torneioId);
        var torneio = await _torneioServico.ObterPorId(torneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Torneios,
            Acao = "FinalizarTorneio",
            Descricao = "Status do torneio alterado para Finalizado via app.",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = GetPerfil(),
            IpAddress = UsuarioIp
        });
        return NoContent();
    }

    [HttpPost("reabrir")]
    public async Task<IActionResult> Reabrir()
    {
        var torneioId = GetTorneioIdClaim() ?? Guid.Empty;
        await _torneioServico.Reabrir(torneioId);
        var torneio = await _torneioServico.ObterPorId(torneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = torneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Torneios,
            Acao = "ReabrirTorneio",
            Descricao = "Status do torneio alterado para Aberto via app.",
            UsuarioNome = UsuarioNome,
            UsuarioPerfil = GetPerfil(),
            IpAddress = UsuarioIp
        });
        return NoContent();
    }

    private async Task<string?> SalvarLogoAsync(IFormFile? logo)
    {
        if (logo == null || logo.Length == 0) return null;
        var ext = Path.GetExtension(logo.FileName).ToLowerInvariant();
        await using var stream = logo.OpenReadStream();
        return await _fileStorage.SalvarAsync(stream, $"{Guid.NewGuid()}{ext}", "logos");
    }

    private static string DescreverAlteracoes(TorneioDto atual, AtualizarTorneioAdminFormDto novo, string? logoUrl)
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

public class TorneioAdminConfiguracaoDto
{
    public string NomeTorneio { get; init; } = null!;
    public string? LogoUrl { get; init; }
    public string? Descricao { get; init; }
    public string? ObservacoesInternas { get; init; }
    public int QtdGanhadores { get; init; }
    public bool UsarFatorMultiplicador { get; init; }
    public bool PermitirCapturaOffline { get; init; }
    public bool ExibirModuloFinanceiro { get; init; }
    public bool ExibirParticipantesPublicos { get; init; }
    public bool ExibirNaListaInicialPublica { get; init; }
    public bool ExibirNaPesquisaPublica { get; init; }
    public bool PremiacaoPorEquipe { get; init; }
    public bool PremiacaoPorMembro { get; init; }
    public bool ApenasMaiorCapturaPorPescador { get; init; }
    public string? CorPrimaria { get; init; }
}

public class AtualizarTorneioAdminFormDto
{
    [Required(ErrorMessage = "O nome do torneio e obrigatorio.")]
    public string NomeTorneio { get; init; } = null!;
    public IFormFile? Logo { get; init; }
    public string? Descricao { get; init; }
    public string? ObservacoesInternas { get; init; }
    [Range(1, 100, ErrorMessage = "Informe entre 1 e 100 ganhadores.")]
    public int QtdGanhadores { get; init; } = 3;
    public bool UsarFatorMultiplicador { get; init; }
    public bool PermitirCapturaOffline { get; init; }
    public bool ExibirModuloFinanceiro { get; init; } = true;
    public bool ExibirParticipantesPublicos { get; init; }
    public bool ExibirNaListaInicialPublica { get; init; } = true;
    public bool ExibirNaPesquisaPublica { get; init; } = true;
    public bool PremiacaoPorEquipe { get; init; } = true;
    public bool PremiacaoPorMembro { get; init; }
    public bool ApenasMaiorCapturaPorPescador { get; init; }
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor invalida. Use o formato #RRGGBB.")]
    public string? CorPrimaria { get; init; }
}
