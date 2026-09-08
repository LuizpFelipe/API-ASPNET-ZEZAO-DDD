using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context; // Certifique-se de que o namespace do seu DbContext está correto

namespace Infrastructure.Repositories
{
    public class ProfessorRepository : IProfessorRepository
    {
        private readonly AppDbContext _context;

        // Injeção do DbContext via construtor
        public ProfessorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Professor>> ObterTodosAsync()
        {
            return await _context.Professores.ToListAsync();
        }

        public async Task<Professor> ObterPorIdAsync(Guid id)
        {
            return await _context.Professores.FindAsync(id);
        }

        public async Task AdicionarAsync(Professor professor)
        {
            await _context.Professores.AddAsync(professor);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Professor professor)
        {
            _context.Professores.Update(professor);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Professor professor)
        {
            _context.Professores.Remove(professor);
            await _context.SaveChangesAsync();
        }
    }
}