using Torneio.Application.DTOs.Membro;

namespace Torneio.Application.Services.Interfaces;

public interface IPescaProImportacaoServico
{
    bool Configurado { get; }
    Task<IReadOnlyList<PescaProPescadorDto>> ListarPescadores();
}
