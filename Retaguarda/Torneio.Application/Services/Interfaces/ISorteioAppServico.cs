using Torneio.Application.DTOs.Sorteio;

namespace Torneio.Application.Services.Interfaces;

public interface ISorteioAppServico
{
    Task<IEnumerable<SorteioEquipeDto>> RealizarSorteio(Guid anoTorneioId);
    Task<IEnumerable<SorteioEquipeDto>> ObterResultado(Guid anoTorneioId);
    Task AjustarPosicao(Guid sorteioEquipeId, int novaPosicao);
    Task LimparSorteio(Guid anoTorneioId);
}
