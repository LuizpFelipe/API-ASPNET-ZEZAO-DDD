using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasOne<Aluno>()
            .WithMany()
            .HasForeignKey(c => c.AlunoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
