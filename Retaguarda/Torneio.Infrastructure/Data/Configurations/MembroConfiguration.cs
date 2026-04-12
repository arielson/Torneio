using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class MembroConfiguration : IEntityTypeConfiguration<Membro>
{
    public void Configure(EntityTypeBuilder<Membro> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.FotoUrl).HasMaxLength(500);

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
