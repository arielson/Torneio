using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class GrupoConfiguration : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Nome).IsRequired().HasMaxLength(100);

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(g => g.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Membros)
            .WithOne()
            .HasForeignKey(m => m.GrupoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.TorneioId);
        builder.HasIndex(g => new { g.TorneioId, g.Nome }).IsUnique();
    }
}
