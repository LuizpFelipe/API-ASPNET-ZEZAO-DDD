using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
    {
        public void Configure(EntityTypeBuilder<Professor> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);
            builder.Property(p => p.Telefone).HasMaxLength(20);

            builder.HasOne<Usuario>()
                   .WithMany()
                   .HasForeignKey(professor => professor.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}