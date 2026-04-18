using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

[AllowAnonymous]
[Route("api/torneios")]
public class TorneiosPublicosController : BaseController
{
    private readonly ITorneioServico _torneioServico;
    public TorneiosPublicosController(ITorneioServico torneioServico) => _torneioServico = torneioServico;

    [HttpGet("recentes")]
    public async Task<IActionResult> Recentes([FromQuery] int limite = 5)
    {
        var lista = await _torneioServico.ListarRecentes(Math.Clamp(limite, 1, 20));
        return Ok(lista);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string q = "")
    {
        var lista = await _torneioServico.BuscarPorTexto(q);
        return Ok(lista);
    }
}
