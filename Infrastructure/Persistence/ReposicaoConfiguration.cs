using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

public class ReposicaoConfiguration : IEntityTypeConfiguration<Reposicao>
{
    public void Configure(EntityTypeBuilder<Reposicao> builder)
    {
        builder.ToTable("Reposicoes");

        builder.HasKey(r => r.Id);

        builder.HasOne<Aluno>()
            .WithMany()
            .HasForeignKey(r => r.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Turma>()
            .WithMany()
            .HasForeignKey(r => r.TurmaOrigemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Turma>()
            .WithMany()
            .HasForeignKey(r => r.TurmaDestinoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
