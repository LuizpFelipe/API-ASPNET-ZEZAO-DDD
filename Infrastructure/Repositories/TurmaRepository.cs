using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

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

        public async Task<int> ContarAlunosNaoInativosAsync(Guid turmaId)
        {
            return await _context.Set<Aluno>().CountAsync(a => a.TurmaId == turmaId && a.Status != StatusAluno.Inativo);
        }

        /// <summary>Move os alunos não inativos para a turma de destino (se informada) e salva tudo junto com a turma já inativada.</summary>
        public async Task SalvarInativacaoAsync(Turma turmaInativada, Guid? turmaDestinoId)
        {
            if (turmaDestinoId.HasValue)
            {
                var destino = await _context.Turmas.FindAsync(turmaDestinoId.Value);
                var alunos = await _context.Set<Aluno>()
                    .Where(a => a.TurmaId == turmaInativada.Id && a.Status != StatusAluno.Inativo)
                    .ToListAsync();
                foreach (var aluno in alunos)
                    aluno.PromoverParaCategoria(destino!.CategoriaId, destino.Id);
            }

            _context.Turmas.Update(turmaInativada);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PossuiVinculosAsync(Guid turmaId)
        {
            return await _context.Set<Aluno>().AnyAsync(a => a.TurmaId == turmaId)
                || await _context.Set<Presenca>().AnyAsync(p => p.TurmaId == turmaId);
        }
    }
}