using Torneio.Application.DTOs.Premio;

namespace Torneio.Application.Services.Interfaces;

public interface IPremioServico
{
    Task<IEnumerable<PremioDto>> ListarPorTorneio(Guid torneioId);
    Task<PremioDto> Criar(Guid torneioId, CriarPremioDto dto);
    Task Atualizar(Guid id, string descricao);
    Task Remover(Guid id);
}
