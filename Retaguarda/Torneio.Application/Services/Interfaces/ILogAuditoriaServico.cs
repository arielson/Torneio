using Torneio.Application.DTOs.Log;

namespace Torneio.Application.Services.Interfaces;

public interface ILogAuditoriaServico
{
    /// <summary>Registra um evento de auditoria. Erros são suprimidos para não quebrar a operação principal.</summary>
    Task Registrar(RegistrarLogDto dto);

    Task<(IEnumerable<LogAuditoriaDto> Itens, int Total)> Listar(
        Guid? torneioId = null,
        string? categoria = null,
        string? usuarioPerfil = null,
        string? busca = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        int pagina = 1,
        int tamanhoPagina = 50);
}
