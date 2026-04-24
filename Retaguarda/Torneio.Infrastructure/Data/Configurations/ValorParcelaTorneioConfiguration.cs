using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class ValorParcelaTorneioConfiguration : IEntityTypeConfiguration<ValorParcelaTorneio>
{
    public void Configure(EntityTypeBuilder<ValorParcelaTorneio> builder)
    {
        builder.ToTable("valores_parcelas_torneio");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TorneioId).IsRequired();
        builder.Property(x => x.NumeroParcela).IsRequired();
        builder.Property(x => x.Valor).HasPrecision(18, 2).IsRequired();
        builder.HasIndex(x => new { x.TorneioId, x.NumeroParcela }).IsUnique();
    }
}
