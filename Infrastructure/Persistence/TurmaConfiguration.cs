using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence
{
    public class TurmaConfiguration : IEntityTypeConfiguration<Turma>
    {
        public void Configure(EntityTypeBuilder<Turma> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Nome).IsRequired().HasMaxLength(100);
            builder.Property(t => t.DiasSemana).HasMaxLength(50);
            builder.Property(t => t.Horario).HasMaxLength(20);

            
        }
    }
}