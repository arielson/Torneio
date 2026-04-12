namespace Torneio.Domain.Interfaces.Services;

public interface ITenantContext
{
    Guid TorneioId { get; }
    string Slug { get; }
    bool EhAdminGeral { get; }
}
