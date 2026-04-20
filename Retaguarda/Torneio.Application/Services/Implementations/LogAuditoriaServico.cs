using Torneio.Application.DTOs.Log;
using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class LogAuditoriaServico : ILogAuditoriaServico
{
    private readonly ILogAuditoriaRepositorio _repositorio;

    public LogAuditoriaServico(ILogAuditoriaRepositorio repositorio)
        => _repositorio = repositorio;

    public async Task Registrar(RegistrarLogDto dto)
    {
        try
        {
            var nome = string.IsNullOrWhiteSpace(dto.UsuarioNome) || dto.UsuarioNome == "—"
                ? null : dto.UsuarioNome;
            var descricao = nome is not null
                ? $"[{nome}] {dto.Descricao}"
                : dto.Descricao;

            var log = LogAuditoria.Criar(
                dto.TorneioId,
                dto.NomeTorneio,
                dto.Categoria,
                dto.Acao,
                descricao,
                dto.UsuarioNome,
                dto.UsuarioPerfil,
                dto.IpAddress);

            await _repositorio.Adicionar(log);
        }
        catch
        {
            // Logging nunca deve quebrar a operacao principal
        }
    }

    public Task<int> LimparTodos() => _repositorio.LimparTodos();

    public async Task<(IEnumerable<LogAuditoriaDto> Itens, int Total)> Listar(
        Guid? torneioId = null,
        string? categoria = null,
        string? usuarioPerfil = null,
        string? busca = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int pagina = 1,
        int tamanhoPagina = 50)
    {
        var (itens, total) = await _repositorio.Listar(
            torneioId, categoria, usuarioPerfil, busca, dataInicio, dataFim, pagina, tamanhoPagina);

        return (itens.Select(ParaDto), total);
    }

    private static LogAuditoriaDto ParaDto(LogAuditoria l) => new()
    {
        Id = l.Id,
        TorneioId = l.TorneioId,
        NomeTorneio = l.NomeTorneio,
        Categoria = l.Categoria,
        Acao = l.Acao,
        Descricao = l.Descricao,
        UsuarioNome = l.UsuarioNome,
        UsuarioPerfil = l.UsuarioPerfil,
        IpAddress = l.IpAddress,
        DataHora = l.DataHora
    };
}
