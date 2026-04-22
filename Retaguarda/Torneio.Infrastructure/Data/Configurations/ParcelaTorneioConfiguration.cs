using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class ParcelaTorneioConfiguration : IEntityTypeConfiguration<ParcelaTorneio>
{
    public void Configure(EntityTypeBuilder<ParcelaTorneio> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TipoParcela).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Valor).HasPrecision(18, 2);
        builder.Property(x => x.Observacao).HasMaxLength(1000);
        builder.Property(x => x.ComprovanteNomeArquivo).HasMaxLength(260);
        builder.Property(x => x.ComprovanteUsuarioNome).HasMaxLength(200);
        builder.Property(x => x.ComprovanteUrl).HasMaxLength(500);
        builder.Property(x => x.ComprovanteContentType).HasMaxLength(120);
        builder.HasIndex(x => new { x.TorneioId, x.MembroId, x.TipoParcela, x.NumeroParcela, x.ReferenciaId }).IsUnique();
    }
}
