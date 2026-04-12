using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.FotoUrl).HasMaxLength(500);
        builder.Property(e => e.Comprimento).HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(e => e.FatorMultiplicador).HasColumnType("numeric(10,4)").IsRequired();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
