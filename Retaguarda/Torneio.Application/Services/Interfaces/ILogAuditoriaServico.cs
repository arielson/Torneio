using Torneio.Application.DTOs.Log;

namespace Torneio.Application.Services.Interfaces;

public interface ILogAuditoriaServico
{
    /// <summary>Registra um evento de auditoria. Erros sao suprimidos para nao quebrar a operacao principal.</summary>
    Task Registrar(RegistrarLogDto dto);
    Task<int> LimparTodos();

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
