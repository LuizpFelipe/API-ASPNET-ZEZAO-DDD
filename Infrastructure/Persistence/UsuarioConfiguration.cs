using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            builder.Property(u => u.NomeUsuario)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.NomeUsuario)
                .IsUnique();

            builder.Property(u => u.SenhaHash)
                .IsRequired();

            builder.Property(u => u.Perfil)
                .HasConversion<string>()
                .IsRequired();
        }
    }
}