using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Repositories
{
    public class TurmaRepository : ITurmaRepository
    {
        private readonly AppDbContext _context;

        public TurmaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Turma>> ObterTodasAsync()
        {
            return await _context.Turmas.ToListAsync();
        }

        public async Task<Turma> ObterPorIdAsync(Guid id)
        {
            return await _context.Turmas.FindAsync(id);
        }

        public async Task<IEnumerable<Turma>> ObterPorCategoriaAsync(Guid categoriaId)
        {
            return await _context.Turmas
                .Where(t => t.CategoriaId == categoriaId)
                .ToListAsync();
        }

        public async Task<int> ContarAlunosPorTurmaAsync(Guid turmaId)
        {
            // Ajuste a lógica de contagem real quando a entidade de Alunos/Matrículas estiver integrada
            await Task.CompletedTask;
            return 0;
        }

        public async Task AdicionarAsync(Turma turma)
        {
            await _context.Turmas.AddAsync(turma);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Turma turma)
        {
            _context.Turmas.Update(turma);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Turma turma)
        {
            _context.Turmas.Remove(turma);
            await _context.SaveChangesAsync();
        }
    }
}