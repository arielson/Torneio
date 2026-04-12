using Torneio.Domain.Interfaces.Services;

namespace Torneio.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid TorneioId { get; private set; } = Guid.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool EhAdminGeral { get; private set; } = false;

    public void DefinirTenant(Guid torneioId, string slug)
    {
        TorneioId = torneioId;
        Slug = slug;
        EhAdminGeral = false;
    }

    public void DefinirAdminGeral(Guid? torneioId = null, string? slug = null)
    {
        EhAdminGeral = true;
        TorneioId = torneioId ?? Guid.Empty;
        Slug = slug ?? string.Empty;
    }
}
