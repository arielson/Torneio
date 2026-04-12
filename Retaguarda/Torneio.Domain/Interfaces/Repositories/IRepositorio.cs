namespace Torneio.Domain.Interfaces.Repositories;

public interface IRepositorio<T> where T : class
{
    Task<T?> ObterPorId(Guid id);
    Task<IEnumerable<T>> ListarTodos();
    Task Adicionar(T entidade);
    Task Atualizar(T entidade);
    Task Remover(Guid id);
}
