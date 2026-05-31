using Torneio.Application.DTOs.Asaas;

namespace Torneio.Application.Services.Interfaces;

public interface ICobrancaAsaasServico
{
    Task<CobrancaAsaasDto> GerarCobranca(GerarCobrancaDto dto);
    Task<CobrancaAsaasDto?> ObterPorParcelaId(Guid parcelaTorneioId);
    Task<IEnumerable<CobrancaAsaasDto>> ListarPorMembro(Guid torneioId, Guid membroId);
    Task<PixQrCodeDto> ObterQrCodePix(Guid parcelaTorneioId);
    Task CancelarCobranca(Guid parcelaTorneioId);
}
