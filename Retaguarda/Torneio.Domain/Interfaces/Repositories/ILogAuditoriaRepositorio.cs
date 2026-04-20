using Torneio.Domain.Entities;

namespace Torneio.Domain.Interfaces.Repositories;

public interface ILogAuditoriaRepositorio
{
    Task Adicionar(LogAuditoria log);
    Task<int> LimparTodos();
    Task<(IEnumerable<LogAuditoria> Itens, int Total)> Listar(
        Guid? torneioId,
        string? categoria,
        string? usuarioPerfil,
        string? busca,
        DateTime? dataInicio,
        DateTime? dataFim,
        int pagina,
        int tamanhoPagina);
}
