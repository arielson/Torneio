using Torneio.Application.DTOs.Sorteio;

namespace Torneio.Application.Services.Interfaces;

public interface ISorteioAppServico
{
    Task<IEnumerable<SorteioEquipeDto>> RealizarSorteio();
    Task<IEnumerable<SorteioEquipeDto>> ObterResultado();
    Task AjustarPosicao(Guid sorteioEquipeId, int novaPosicao);
    Task LimparSorteio();
}
