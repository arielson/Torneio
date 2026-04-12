using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class AnoTorneioConfiguration : IEntityTypeConfiguration<AnoTorneio>
{
    public void Configure(EntityTypeBuilder<AnoTorneio> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Ano).IsRequired();
        builder.Property(e => e.Status).IsRequired();

        builder.HasIndex(e => new { e.TorneioId, e.Ano }).IsUnique();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
