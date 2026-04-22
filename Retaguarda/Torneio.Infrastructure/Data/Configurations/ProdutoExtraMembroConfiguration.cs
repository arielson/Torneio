using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class ProdutoExtraMembroConfiguration : IEntityTypeConfiguration<ProdutoExtraMembro>
{
    public void Configure(EntityTypeBuilder<ProdutoExtraMembro> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantidade).HasPrecision(18, 2);
        builder.Property(x => x.ValorCobrado).HasPrecision(18, 2);
        builder.Property(x => x.Observacao).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ProdutoExtraTorneioId, x.MembroId }).IsUnique();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(x => x.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
