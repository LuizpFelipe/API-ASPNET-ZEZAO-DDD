using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class PresencaConfiguration : IEntityTypeConfiguration<Presenca>
    {
        public void Configure(EntityTypeBuilder<Presenca> builder)
        {
            builder.ToTable("Presencas");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Data).IsRequired();
            builder.Property(p => p.Presente).IsRequired();

            builder.HasIndex(p => new { p.AlunoId, p.TurmaId, p.Data })
                .IsUnique();


            builder.HasIndex(p => new { p.TurmaId, p.Data });

            builder.HasOne<Aluno>()
                .WithMany()
                .HasForeignKey(p => p.AlunoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Turma>()
                .WithMany()
                .HasForeignKey(p => p.TurmaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}