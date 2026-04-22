using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class CustoTorneioConfiguration : IEntityTypeConfiguration<CustoTorneio>
{
    public void Configure(EntityTypeBuilder<CustoTorneio> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Quantidade).HasPrecision(18, 2);
        builder.Property(x => x.ValorUnitario).HasPrecision(18, 2);
        builder.Property(x => x.ValorTotal).HasPrecision(18, 2);
        builder.Property(x => x.Responsavel).HasMaxLength(200);
        builder.Property(x => x.Observacao).HasMaxLength(1000);
    }
}
