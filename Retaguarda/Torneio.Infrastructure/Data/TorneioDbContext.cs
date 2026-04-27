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
    public DbSet<FiscalEquipe> FiscaisEquipes { get; set; }
    public DbSet<Equipe> Equipes { get; set; }
    public DbSet<Membro> Membros { get; set; }
    public DbSet<RegistroPublicoMembro> RegistrosPublicosMembros { get; set; }
    public DbSet<EspeciePeixe> EspeciesPeixe { get; set; }
    public DbSet<Item> Itens { get; set; }
    public DbSet<Patrocinador> Patrocinadores { get; set; }
    public DbSet<ParcelaTorneio> ParcelasTorneio { get; set; }
    public DbSet<ValorParcelaTorneio> ValoresParcelas { get; set; }
    public DbSet<ProdutoExtraTorneio> ProdutosExtrasTorneio { get; set; }
    public DbSet<ProdutoExtraMembro> ProdutosExtrasMembros { get; set; }
    public DbSet<DoacaoPatrocinador> DoacoesPatrocinadores { get; set; }
    public DbSet<CustoTorneio> CustosTorneio { get; set; }
    public DbSet<ChecklistTorneioItem> ChecklistTorneioItens { get; set; }
    public DbSet<Captura> Capturas { get; set; }
    public DbSet<SorteioEquipe> SorteiosEquipe { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<GrupoMembro> GruposMembros { get; set; }
    public DbSet<SorteioGrupo> SorteiosGrupo { get; set; }
    public DbSet<Premio> Premios { get; set; }
    public DbSet<LogAuditoria> Logs { get; set; }

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

        modelBuilder.Entity<FiscalEquipe>()
            .HasQueryFilter(e =>
                _tenantContext.EhAdminGeral ||
                (e.Fiscal.TorneioId == _tenantContext.TorneioId &&
                 e.Equipe.TorneioId == _tenantContext.TorneioId));

        modelBuilder.Entity<Equipe>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Membro>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<RegistroPublicoMembro>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Item>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Patrocinador>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<ParcelaTorneio>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<ProdutoExtraTorneio>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<ProdutoExtraMembro>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<DoacaoPatrocinador>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<CustoTorneio>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<ChecklistTorneioItem>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Captura>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<SorteioEquipe>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Grupo>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<GrupoMembro>()
            .HasQueryFilter(e =>
                _tenantContext.EhAdminGeral ||
                (e.Membro != null &&
                 e.Membro.TorneioId == _tenantContext.TorneioId));

        modelBuilder.Entity<SorteioGrupo>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);

        modelBuilder.Entity<Premio>()
            .HasQueryFilter(e => e.TorneioId == _tenantContext.TorneioId || _tenantContext.EhAdminGeral);
    }
}
