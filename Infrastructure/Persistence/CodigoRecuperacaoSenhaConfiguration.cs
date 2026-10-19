using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class CodigoRecuperacaoSenhaConfiguration : IEntityTypeConfiguration<CodigoRecuperacaoSenha>
    {
        public void Configure(EntityTypeBuilder<CodigoRecuperacaoSenha> builder)
        {
            builder.ToTable("CodigosRecuperacaoSenha");

            builder.HasKey(codigo => codigo.Id);

            builder.Property(codigo => codigo.CodigoHash)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(codigo => codigo.TokenHash)
                .HasMaxLength(64);

            builder.HasIndex(codigo => codigo.TokenHash);
            builder.HasIndex(codigo => new { codigo.UsuarioId, codigo.CriadoEm });

            builder.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(codigo => codigo.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}