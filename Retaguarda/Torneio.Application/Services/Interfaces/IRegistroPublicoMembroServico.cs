using Torneio.Application.DTOs.Membro;
using Torneio.Application.DTOs.RegistroPublicoMembro;

namespace Torneio.Application.Services.Interfaces;

public interface IRegistroPublicoMembroServico
{
    Task<RegistroPublicoMembroSolicitadoDto> SolicitarCodigo(Guid torneioId, string nomeTorneio, SolicitarRegistroPublicoMembroDto dto, string? ipAddress);
    Task<MembroDto> ConfirmarCodigo(Guid torneioId, string nomeTorneio, ConfirmarRegistroPublicoMembroDto dto, string? ipAddress);
}
