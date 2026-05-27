using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

[Route("api/{slug}/mensagens")]
public class MensagemPublicaController : BaseController
{
    private readonly IMensagemTorneioServico _servico;

    public MensagemPublicaController(IMensagemTorneioServico servico)
    {
        _servico = servico;
    }

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.ListarAsync());
}
