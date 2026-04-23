using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/financeiro/doacoes")]
public class DoacoesPatrocinadorController : BaseController
{
    private readonly IDoacaoPatrocinadorServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly ILogAuditoriaServico _logAuditoriaServico;
    private readonly ITorneioServico _torneioServico;

    public DoacoesPatrocinadorController(
        IDoacaoPatrocinadorServico servico,
        TenantContext tenantContext,
        ILogAuditoriaServico logAuditoriaServico,
        ITorneioServico torneioServico)
    {
        _servico = servico;
        _tenantContext = tenantContext;
        _logAuditoriaServico = logAuditoriaServico;
        _torneioServico = torneioServico;
    }

    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await _servico.Listar(_tenantContext.TorneioId));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarDoacaoPatrocinadorDto dto)
    {
        var criada = await _servico.Criar(new CriarDoacaoPatrocinadorDto
        {
            TorneioId = _tenantContext.TorneioId,
            PatrocinadorId = dto.PatrocinadorId,
            NomePatrocinador = dto.NomePatrocinador,
            Tipo = dto.Tipo,
            Descricao = dto.Descricao,
            Quantidade = dto.Quantidade,
            Valor = dto.Valor,
            Observacao = dto.Observacao,
            DataDoacao = dto.DataDoacao
        });
        await RegistrarLog(
            "CriarDoacaoPatrocinadorApp",
            $"Doacao registrada pelo app | Patrocinador: {criada.NomePatrocinador} | Tipo: {criada.Tipo} | Descricao: {criada.Descricao} | Valor: {(criada.Valor ?? 0):0.00}");
        return Ok(criada);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarDoacaoPatrocinadorDto dto)
    {
        await _servico.Atualizar(id, dto);
        await RegistrarLog(
            "AtualizarDoacaoPatrocinadorApp",
            $"Doacao atualizada pelo app | Doacao: {id} | Tipo: {dto.Tipo} | Descricao: {dto.Descricao} | Valor: {(dto.Valor ?? 0):0.00}");
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        await RegistrarLog("RemoverDoacaoPatrocinadorApp", $"Doacao removida pelo app | Doacao: {id}");
        return NoContent();
    }

    private async Task RegistrarLog(string acao, string descricao)
    {
        var torneio = await _torneioServico.ObterPorId(_tenantContext.TorneioId);
        await _logAuditoriaServico.Registrar(new RegistrarLogDto
        {
            TorneioId = _tenantContext.TorneioId,
            NomeTorneio = torneio?.NomeTorneio,
            Categoria = CategoriaLog.Financeiro,
            Acao = acao,
            Descricao = descricao,
            UsuarioNome = User.Identity?.Name ?? "-",
            UsuarioPerfil = GetPerfil(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }
}
