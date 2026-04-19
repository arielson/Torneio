using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Torneio.Domain.Entities;
using Torneio.Domain.Interfaces.Services;

namespace Torneio.Infrastructure.Data;

public class TorneioDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public TorneioDbContext(DbContextOptions<TorneioDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<TorneioEntity> Torneiros { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<AdminGeral> AdminsGeral { get; set; }
    public DbSet<AdminTorneio> AdminsTorneio { get; set; }
    public DbSet<Fiscal> Fiscais { get; set; }
    public DbSet<Equipe> Equipes { get; set; }
    public DbSet<Membro> Membros { get; set; }
    public DbSet<Item> Itens { get; set; }
    public DbSet<Captura> Capturas { get; set; }
    public DbSet<SorteioEquipe> SorteiosEquipe { get; set; }
    public DbSet<Premio> Premios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TorneioDbContext).Assembly);

        // Query filters por TorneioId — bypass total quando EhAdminGeral
        modelBuilder.Entity<AdminTorneio>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Fiscal>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Equipe>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Membro>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Item>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Captura>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<SorteioEquipe>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Premio>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);
    }
}
