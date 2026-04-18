using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class PremioConfiguration : IEntityTypeConfiguration<Premio>
{
    public void Configure(EntityTypeBuilder<Premio> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Posicao).IsRequired();
        builder.Property(e => e.Descricao).IsRequired().HasMaxLength(500);

        // Uma posição por torneio
        builder.HasIndex(e => new { e.TorneioId, e.Posicao }).IsUnique();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
