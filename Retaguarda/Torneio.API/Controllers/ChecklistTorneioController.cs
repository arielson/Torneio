using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/financeiro/checklist")]
public class ChecklistTorneioController : BaseController
{
    private readonly IChecklistTorneioItemServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly ILogAuditoriaServico _logAuditoriaServico;
    private readonly ITorneioServico _torneioServico;

    public ChecklistTorneioController(
        IChecklistTorneioItemServico servico,
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
    public async Task<IActionResult> Criar([FromBody] CriarChecklistTorneioItemDto dto)
    {
        var criado = await _servico.Criar(new CriarChecklistTorneioItemDto
        {
            TorneioId = _tenantContext.TorneioId,
            Item = dto.Item,
            Data = dto.Data,
            Responsavel = dto.Responsavel,
            Concluido = dto.Concluido
        });
        await RegistrarLog(
            "CriarChecklistApp",
            $"Item de checklist criado pelo app | Item: {criado.Item} | Responsavel: {criado.Responsavel ?? "-"} | Concluido: {criado.Concluido}");
        return Ok(criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarChecklistTorneioItemDto dto)
    {
        await _servico.Atualizar(id, dto);
        await RegistrarLog(
            "AtualizarChecklistApp",
            $"Item de checklist atualizado pelo app | Item: {id} | Descricao: {dto.Item} | Concluido: {dto.Concluido}");
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        await RegistrarLog("RemoverChecklistApp", $"Item de checklist removido pelo app | Item: {id}");
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
