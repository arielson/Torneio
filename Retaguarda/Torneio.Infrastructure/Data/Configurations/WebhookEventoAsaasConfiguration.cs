using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class WebhookEventoAsaasConfiguration : IEntityTypeConfiguration<WebhookEventoAsaas>
{
    public void Configure(EntityTypeBuilder<WebhookEventoAsaas> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventoId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TipoEvento).IsRequired().HasMaxLength(80);
        builder.Property(e => e.AsaasPaymentId).HasMaxLength(100);
        builder.Property(e => e.PayloadJson).IsRequired();
        builder.Property(e => e.ErroProcessamento).HasMaxLength(2000);

        // Garante idempotência: cada eventoId processado apenas uma vez
        builder.HasIndex(e => e.EventoId).IsUnique();
        builder.HasIndex(e => new { e.Processado, e.RecebidoEm });
    }
}
