using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Torneio.Infrastructure.Services;

namespace Torneio.Infrastructure.Data;

/// <summary>
/// Factory para uso exclusivo pelo tooling do EF Core (dotnet ef migrations add / update).
/// A connection string usada aqui é apenas para design-time; em runtime o valor vem do appsettings.json.
/// </summary>
public class TorneioDbContextFactory : IDesignTimeDbContextFactory<TorneioDbContext>
{
    // Lê a variável de ambiente TORNEIO_DB se disponível; caso contrário usa o padrão local.
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TORNEIO_DB")
        ?? "Host=localhost;Database=torneio;Username=postgres;Password=Httpr0x1!";

    public TorneioDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TorneioDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        var tenant = new TenantContext();
        tenant.DefinirAdminGeral();

        return new TorneioDbContext(options, tenant);
    }
}
