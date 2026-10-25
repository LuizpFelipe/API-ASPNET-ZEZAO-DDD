using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence;

public class ParcelaConfiguration : IEntityTypeConfiguration<Parcela>
{
    public void Configure(EntityTypeBuilder<Parcela> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne<Contrato>()
            .WithMany()
            .HasForeignKey(p => p.ContratoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
