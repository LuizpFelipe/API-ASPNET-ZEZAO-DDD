using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;

namespace Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context; 

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categoria>> ObterTodasAsync()
        {
            return await _context.Set<Categoria>().AsNoTracking().ToListAsync();
        }

        public async Task<Categoria?> ObterPorIdAsync(Guid id)
        {
            return await _context.Set<Categoria>().FindAsync(id);
        }

        public async Task<Categoria?> ObterPorIdadeAsync(int idade)
        {
            return await _context.Set<Categoria>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => idade >= c.IdadeMin && idade <= c.IdadeMax);
        }

        public async Task<bool> ExisteSobreposicaoDeIdadeAsync(int idadeMin, int idadeMax, Guid? ignorarId = null)
        {
            var query = _context.Set<Categoria>().AsQueryable();

            if (ignorarId.HasValue)
            {
                query = query.Where(c => c.Id != ignorarId.Value);
            }

            return await query.AnyAsync(c =>
                (idadeMin >= c.IdadeMin && idadeMin <= c.IdadeMax) ||
                (idadeMax >= c.IdadeMin && idadeMax <= c.IdadeMax) ||
                (idadeMin <= c.IdadeMin && idadeMax >= c.IdadeMax));
        }

        public async Task AdicionarAsync(Categoria categoria)
        {
            await _context.Set<Categoria>().AddAsync(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Categoria categoria)
        {
            _context.Set<Categoria>().Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Categoria categoria)
        {
            _context.Set<Categoria>().Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}