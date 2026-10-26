using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class AvisoCobrancaConfiguration : IEntityTypeConfiguration<AvisoCobranca>
    {
        public void Configure(EntityTypeBuilder<AvisoCobranca> builder)
        {
            builder.ToTable("AvisoCobrancas");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Destinatario)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(a => a.Parcela)
                .WithMany()
                .HasForeignKey(a => a.ParcelaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}