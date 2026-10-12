using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CoordenadorRepository : ICoordenadorRepository
    {
        private readonly AppDbContext _context;

        public CoordenadorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coordenador>> ObterTodosAsync()
        {
            return await _context.Set<Coordenador>()
                .Include(c => c.Usuario)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Coordenador?> ObterPorIdAsync(Guid id)
        {
            return await _context.Set<Coordenador>()
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AdicionarAsync(Coordenador coordenador)
        {
            await _context.Set<Coordenador>().AddAsync(coordenador);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Coordenador coordenador)
        {
            // O Coordenador e o Usuario já vêm rastreados de ObterPorIdAsync, então a troca
            // de e-mail feita no Usuario também é gravada neste SaveChanges.
            _context.Entry(coordenador).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Coordenador coordenador)
        {
            _context.Set<Coordenador>().Remove(coordenador);

            if (coordenador.Usuario != null)
                _context.Set<Usuario>().Remove(coordenador.Usuario);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailJaCadastradoAsync(string email, Guid? ignorarUsuarioId = null)
        {
            return await _context.Set<Usuario>()
                .AnyAsync(u => u.Email == email
                               && (ignorarUsuarioId == null || u.Id != ignorarUsuarioId));
        }
    }
}