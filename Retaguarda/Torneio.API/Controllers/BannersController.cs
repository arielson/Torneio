using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.Services.Interfaces;

namespace Torneio.API.Controllers;

[AllowAnonymous]
[Route("api/banners")]
public class BannersController : BaseController
{
    private readonly IBannerServico _bannerServico;
    public BannersController(IBannerServico bannerServico) => _bannerServico = bannerServico;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var lista = await _bannerServico.ListarAtivos();
        return Ok(lista);
    }
}
