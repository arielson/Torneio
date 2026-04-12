using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class FiscalConfiguration : IEntityTypeConfiguration<Fiscal>
{
    public void Configure(EntityTypeBuilder<Fiscal> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Usuario).IsRequired().HasMaxLength(100);
        builder.Property(e => e.SenhaHash).IsRequired().HasMaxLength(200);
        builder.Property(e => e.FotoUrl).HasMaxLength(500);

        builder.HasIndex(e => new { e.Usuario, e.TorneioId }).IsUnique();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AnoTorneio>()
            .WithMany()
            .HasForeignKey(e => e.AnoTorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
