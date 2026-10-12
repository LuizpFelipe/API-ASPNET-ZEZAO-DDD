using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class CoordenadorConfiguration : IEntityTypeConfiguration<Coordenador>
    {
        public void Configure(EntityTypeBuilder<Coordenador> builder)
        {
            builder.ToTable("Coordenadores");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.UsuarioId)
                .IsRequired();

            // 1:1 com Usuario (FK Coordenadores.UsuarioId -> Usuarios.Id).
            builder.HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Coordenador>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.UsuarioId)
                .IsUnique();
        }
    }
}