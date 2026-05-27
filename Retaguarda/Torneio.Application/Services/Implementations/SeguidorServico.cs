using Torneio.Application.Services.Interfaces;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Application.Services.Implementations;

public class SeguidorServico : ISeguidorServico
{
    private readonly ISeguidorTorneioRepositorio _repositorio;

    public SeguidorServico(ISeguidorTorneioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task RegistrarAsync(Guid torneioId, string deviceToken, string plataforma)
    {
        var existente = await _repositorio.ObterPorToken(torneioId, deviceToken);
        if (existente is not null) return;

        var seguidor = SeguidorTorneio.Criar(torneioId, deviceToken, plataforma);
        await _repositorio.Adicionar(seguidor);
    }

    public async Task RemoverAsync(Guid torneioId, string deviceToken)
    {
        await _repositorio.RemoverPorToken(torneioId, deviceToken);
    }
}
