using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torneio.Application.DTOs.Financeiro;
using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Infrastructure.Services;

namespace Torneio.API.Controllers;

[Authorize(Policy = "AdminTorneio")]
[Route("api/{slug}/financeiro/custos")]
public class CustosTorneioController : BaseController
{
    private readonly ICustoTorneioServico _servico;
    private readonly TenantContext _tenantContext;
    private readonly ILogAuditoriaServico _logAuditoriaServico;
    private readonly ITorneioServico _torneioServico;

    public CustosTorneioController(
        ICustoTorneioServico servico,
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
    public async Task<IActionResult> Criar([FromBody] CriarCustoTorneioDto dto)
    {
        var criado = await _servico.Criar(new CriarCustoTorneioDto
        {
            TorneioId = _tenantContext.TorneioId,
            Categoria = dto.Categoria,
            Descricao = dto.Descricao,
            Quantidade = dto.Quantidade,
            ValorUnitario = dto.ValorUnitario,
            Responsavel = dto.Responsavel,
            Observacao = dto.Observacao
        });
        await RegistrarLog(
            "CriarCustoApp",
            $"Custo criado pelo app | Categoria: {criado.Categoria} | Descricao: {criado.Descricao} | Valor total: {criado.ValorTotal:0.00}");
        return Ok(criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCustoTorneioDto dto)
    {
        await _servico.Atualizar(id, dto);
        await RegistrarLog(
            "AtualizarCustoApp",
            $"Custo atualizado pelo app | Custo: {id} | Categoria: {dto.Categoria} | Descricao: {dto.Descricao} | Quantidade: {dto.Quantidade:0.##} | Valor unitario: {dto.ValorUnitario:0.00}");
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _servico.Remover(id);
        await RegistrarLog("RemoverCustoApp", $"Custo removido pelo app | Custo: {id}");
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
