using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class CobrancaAsaasConfiguration : IEntityTypeConfiguration<CobrancaAsaas>
{
    public void Configure(EntityTypeBuilder<CobrancaAsaas> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AsaasPaymentId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.AsaasCustomerId).HasMaxLength(100);
        builder.Property(e => e.AsaasInvoiceUrl).HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.FormaPagamento).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ValorOriginal).HasPrecision(18, 2);
        builder.Property(e => e.TaxaAsaas).HasPrecision(18, 2);

        builder.HasIndex(e => e.AsaasPaymentId).IsUnique();
        builder.HasIndex(e => new { e.TorneioId, e.MembroId });
        builder.HasIndex(e => e.ParcelaTorneioId);

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Membro>()
            .WithMany()
            .HasForeignKey(e => e.MembroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ParcelaTorneio>()
            .WithMany()
            .HasForeignKey(e => e.ParcelaTorneioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
