using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;

namespace Torneio.Web.Controllers;

[Authorize(Policy = "AdminGeral")]
[Route("admin/logs")]
public class LogsController : Controller
{
    private readonly ILogAuditoriaServico _logServico;
    private readonly ITorneioServico _torneioServico;

    public LogsController(ILogAuditoriaServico logServico, ITorneioServico torneioServico)
    {
        _logServico = logServico;
        _torneioServico = torneioServico;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        Guid? torneioId       = null,
        string? categoria     = null,
        string? usuarioPerfil = null,
        string? busca         = null,
        DateTime? dataInicio  = null,
        DateTime? dataFim     = null,
        int pagina            = 1)
    {
        const int tamanhoPagina = 50;

        var (itens, total) = await _logServico.Listar(
            torneioId, categoria, usuarioPerfil, busca, dataInicio, dataFim, pagina, tamanhoPagina);

        ViewBag.Torneios      = await _torneioServico.ListarTodos();
        ViewBag.Categorias    = CategoriaLog.Todas;
        ViewBag.Perfis        = new[] { "AdminGeral", "AdminTorneio", "Fiscal" };
        ViewBag.TorneioId     = torneioId;
        ViewBag.Categoria     = categoria;
        ViewBag.UsuarioPerfil = usuarioPerfil;
        ViewBag.Busca         = busca;
        ViewBag.DataInicio    = dataInicio?.ToString("yyyy-MM-dd");
        ViewBag.DataFim       = dataFim?.ToString("yyyy-MM-dd");
        ViewBag.Pagina        = pagina;
        ViewBag.TotalPaginas  = (int)Math.Ceiling(total / (double)tamanhoPagina);
        ViewBag.Total         = total;

        return View(itens);
    }
}
