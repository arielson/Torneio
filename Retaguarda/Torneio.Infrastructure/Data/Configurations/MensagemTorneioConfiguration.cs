using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class MensagemTorneioConfiguration : IEntityTypeConfiguration<MensagemTorneio>
{
    public void Configure(EntityTypeBuilder<MensagemTorneio> builder)
    {
        builder.ToTable("mensagem_torneio");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TorneioId).IsRequired();
        builder.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Corpo).IsRequired().HasColumnType("text");
        builder.Property(e => e.CriadoPor).IsRequired().HasMaxLength(200);
        builder.Property(e => e.CriadoEm).IsRequired();
    }
}
