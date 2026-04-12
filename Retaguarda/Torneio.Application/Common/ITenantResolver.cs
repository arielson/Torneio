using Torneio.Domain.Entities;

namespace Torneio.Application.Common;

public interface ITenantResolver
{
    Task<TorneioEntity?> ResolverAsync(string slug);
}
