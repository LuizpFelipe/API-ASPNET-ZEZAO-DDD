using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class EquipeCampeonatoConfiguration : IEntityTypeConfiguration<EquipeCampeonato>
    {
        public void Configure(EntityTypeBuilder<EquipeCampeonato> builder)
        {
            builder.ToTable("EquipesCampeonato");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(e => new { e.CampeonatoId, e.Nome }).IsUnique();

            builder.HasOne<Campeonato>()
                .WithMany()
                .HasForeignKey(e => e.CampeonatoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class JogoCampeonatoConfiguration : IEntityTypeConfiguration<JogoCampeonato>
    {
        public void Configure(EntityTypeBuilder<JogoCampeonato> builder)
        {
            builder.ToTable("JogosCampeonato");
            builder.HasKey(j => j.Id);

            builder.HasIndex(j => new { j.CampeonatoId, j.Fase, j.Ordem }).IsUnique();

            builder.HasOne<Campeonato>()
                .WithMany()
                .HasForeignKey(j => j.CampeonatoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<EquipeCampeonato>()
                .WithMany()
                .HasForeignKey(j => j.EquipeAId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<EquipeCampeonato>()
                .WithMany()
                .HasForeignKey(j => j.EquipeBId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<EquipeCampeonato>()
                .WithMany()
                .HasForeignKey(j => j.EquipeVencedoraId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class EstatisticaJogoConfiguration : IEntityTypeConfiguration<EstatisticaJogo>
    {
        public void Configure(EntityTypeBuilder<EstatisticaJogo> builder)
        {
            builder.ToTable("EstatisticasJogo");
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => new { e.JogoId, e.AlunoId }).IsUnique();

            builder.HasOne<JogoCampeonato>()
                .WithMany()
                .HasForeignKey(e => e.JogoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Aluno>()
                .WithMany()
                .HasForeignKey(e => e.AlunoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
