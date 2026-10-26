using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AvisoCobrancaRepository : IAvisoCobrancaRepository
    {
        private readonly AppDbContext _context;

        public AvisoCobrancaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(AvisoCobranca avisoCobranca)
        {
            await _context.Set<AvisoCobranca>().AddAsync(avisoCobranca);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AvisoCobranca>> ObterPorParcelaAsync(Guid parcelaId)
        {
            return await _context.Set<AvisoCobranca>()
                .Where(a => a.ParcelaId == parcelaId)
                .ToListAsync();
        }
    }
}