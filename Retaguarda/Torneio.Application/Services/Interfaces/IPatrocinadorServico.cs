using Torneio.Application.DTOs.Patrocinador;

namespace Torneio.Application.Services.Interfaces;

public interface IPatrocinadorServico
{
    Task<PatrocinadorDto?> ObterPorId(Guid id);
    Task<IEnumerable<PatrocinadorDto>> ListarPorTorneio(Guid torneioId);
    Task<PatrocinadorDto> Criar(CriarPatrocinadorDto dto);
    Task Atualizar(Guid id, AtualizarPatrocinadorDto dto);
    Task Remover(Guid id);
}
