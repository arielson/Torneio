using Torneio.Application.DTOs.Equipe;
using Torneio.Application.DTOs.Membro;

namespace Torneio.Application.Services.Interfaces;

public interface IEquipeServico
{
    Task<EquipeDto?> ObterPorId(Guid id);
    Task<IEnumerable<EquipeDto>> ListarTodos();
    Task<EquipeDto> Criar(CriarEquipeDto dto);
    Task Atualizar(Guid id, AtualizarEquipeDto dto);
    Task Remover(Guid id);
    Task AdicionarMembro(Guid equipeId, Guid membroId);
    Task RemoverMembro(Guid equipeId, Guid membroId);
    Task<(EquipeDto Origem, EquipeDto Destino, MembroDto Membro)> ReorganizarMembroEmergencia(Guid membroId, Guid equipeDestinoId);
    Task<IEnumerable<EquipeDto>> ListarPorTorneioExterno(Guid torneioId);
    Task<int> ImportarDeOutroTorneio(Guid sourceTorneioId, IEnumerable<Guid> equipeIds);
}
