using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("Categorias");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.IdadeMin)
                .IsRequired();

            builder.Property(c => c.IdadeMax)
                .IsRequired();

            builder.HasIndex(c => c.Nome).IsUnique();
        }
    }
}