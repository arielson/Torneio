using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Categoria).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Acao).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Descricao).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.UsuarioNome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.UsuarioPerfil).IsRequired().HasMaxLength(50);
        builder.Property(e => e.NomeTorneio).HasMaxLength(200);
        builder.Property(e => e.IpAddress).HasMaxLength(50);
        builder.Property(e => e.DataHora).IsRequired();

        // Índices para os filtros mais comuns
        builder.HasIndex(e => e.DataHora);
        builder.HasIndex(e => e.TorneioId);
        builder.HasIndex(e => e.Categoria);
    }
}
