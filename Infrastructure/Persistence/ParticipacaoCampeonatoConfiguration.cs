using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class ParticipacaoCampeonatoConfiguration : IEntityTypeConfiguration<ParticipacaoCampeonato>
    {
        public void Configure(EntityTypeBuilder<ParticipacaoCampeonato> builder)
        {
            builder.ToTable("ParticipacoesCampeonato");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Posicao)
                .HasMaxLength(50);

            builder.HasOne<Campeonato>()
                .WithMany()
                .HasForeignKey(p => p.CampeonatoId)
                .IsRequired();

            builder.HasOne<Aluno>()
                .WithMany()
                .HasForeignKey(p => p.AlunoId)
                .IsRequired();
        }
    }
}
