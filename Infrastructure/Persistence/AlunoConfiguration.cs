using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

public class AlunoConfiguration : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.NomeCompleto, a.DataNascimento }).IsUnique();

        builder.HasOne(aluno => aluno.Categoria)
               .WithMany()
               .HasForeignKey(aluno => aluno.CategoriaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Turma>()
               .WithMany()
               .HasForeignKey(aluno => aluno.TurmaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.ResponsavelCpf).HasMaxLength(11);
    }
}