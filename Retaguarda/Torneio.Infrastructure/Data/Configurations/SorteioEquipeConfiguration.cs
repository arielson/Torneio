using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class SorteioEquipeConfiguration : IEntityTypeConfiguration<SorteioEquipe>
{
    public void Configure(EntityTypeBuilder<SorteioEquipe> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Posicao).IsRequired();

        builder.HasOne<TorneioEntity>()
            .WithMany()
            .HasForeignKey(e => e.TorneioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Equipe>()
            .WithMany()
            .HasForeignKey(e => e.EquipeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Membro>()
            .WithMany()
            .HasForeignKey(e => e.MembroId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cada membro só pode estar em uma equipe por torneio
        builder.HasIndex(e => new { e.TorneioId, e.MembroId }).IsUnique();
        builder.HasIndex(e => e.TorneioId);
        builder.HasIndex(e => e.EquipeId);
    }
}
