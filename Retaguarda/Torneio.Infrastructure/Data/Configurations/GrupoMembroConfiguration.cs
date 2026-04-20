using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Torneio.Domain.Entities;

namespace Torneio.Infrastructure.Data.Configurations;

public class GrupoMembroConfiguration : IEntityTypeConfiguration<GrupoMembro>
{
    public void Configure(EntityTypeBuilder<GrupoMembro> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.Membro)
            .WithMany()
            .HasForeignKey(m => m.MembroId)
            .OnDelete(DeleteBehavior.Restrict);

        // Um membro não pode estar em dois grupos do mesmo torneio — via unique por grupo
        builder.HasIndex(m => new { m.GrupoId, m.MembroId }).IsUnique();
        builder.HasIndex(m => m.MembroId);
    }
}
