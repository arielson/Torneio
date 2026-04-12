using Torneio.Application.Common;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Infrastructure.Services;

public class TenantResolver : ITenantResolver
{
    private readonly ITorneioRepositorio _repositorio;

    public TenantResolver(ITorneioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<TorneioEntity?> ResolverAsync(string slug) =>
        await _repositorio.ObterPorSlug(slug);
}
