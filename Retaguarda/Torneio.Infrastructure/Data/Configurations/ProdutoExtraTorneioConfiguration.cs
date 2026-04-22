using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class ProdutoExtraTorneioConfiguration : IEntityTypeConfiguration<ProdutoExtraTorneio>
{
    public void Configure(EntityTypeBuilder<ProdutoExtraTorneio> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Descricao).HasMaxLength(1000);
        builder.Property(x => x.Valor).HasPrecision(18, 2);

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(x => x.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
