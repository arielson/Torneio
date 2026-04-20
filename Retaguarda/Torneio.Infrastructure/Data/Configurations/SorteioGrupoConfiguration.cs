using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class SorteioGrupoConfiguration : IEntityTypeConfiguration<SorteioGrupo>
{
    public void Configure(EntityTypeBuilder<SorteioGrupo> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Posicao).IsRequired();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(s => s.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Grupo>()
            .WithMany()
            .HasForeignKey(s => s.GrupoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipe>()
            .WithMany()
            .HasForeignKey(s => s.EquipeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cada grupo só pode sortear uma equipe por torneio
        builder.HasIndex(s => new { s.TorneioId, s.GrupoId }).IsUnique();
        // Cada equipe só pode ser sorteada uma vez por torneio
        builder.HasIndex(s => new { s.TorneioId, s.EquipeId }).IsUnique();
        builder.HasIndex(s => s.TorneioId);
    }
}
