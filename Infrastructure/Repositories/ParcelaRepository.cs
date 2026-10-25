using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

public class ParcelaRepository : IParcelaRepository
{
    private readonly AppDbContext _context;

    public ParcelaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarVariasAsync(IEnumerable<Parcela> parcelas)
    {
        await _context.Parcelas.AddRangeAsync(parcelas);
        await _context.SaveChangesAsync();
    }
}
