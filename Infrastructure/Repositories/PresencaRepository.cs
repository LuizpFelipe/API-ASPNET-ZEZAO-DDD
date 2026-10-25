using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PresencaRepository : IPresencaRepository
    {
        private readonly AppDbContext _context;

        public PresencaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Professor?> ObterProfessorPorNomeUsuarioAsync(string nomeUsuario)
        {
            return await (from p in _context.Set<Professor>()
                          join u in _context.Set<Usuario>() on p.UsuarioId equals u.Id
                          where u.NomeUsuario == nomeUsuario
                          select p)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Turma>> ObterTurmasAsync(Guid? professorId)
        {
            IQueryable<Turma> query = _context.Set<Turma>().AsNoTracking();

            if (professorId.HasValue)
                query = query.Where(t => t.ProfessorId == professorId.Value);

            return await query.OrderBy(t => t.Nome).ToListAsync();
        }

        public async Task<Turma?> ObterTurmaPorIdAsync(Guid turmaId)
        {
            return await _context.Set<Turma>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == turmaId);
        }

        public async Task<IEnumerable<Aluno>> ObterAlunosDaTurmaAsync(Guid turmaId)
        {
            return await _context.Set<Aluno>()
                .AsNoTracking()
                .Where(a => a.TurmaId == turmaId && a.Status != StatusAluno.Inativo)
                .OrderBy(a => a.NomeCompleto)
                .ToListAsync();
        }

        public async Task<List<Presenca>> ObterPresencasAsync(Guid turmaId, DateOnly data)
        {

            return await _context.Set<Presenca>()
                .Where(p => p.TurmaId == turmaId && p.Data == data)
                .ToListAsync();
        }

        public async Task<List<Presenca>> ObterPresencasDoMesAsync(Guid turmaId, DateOnly inicio, DateOnly fim)
        {
            return await _context.Set<Presenca>()
                .AsNoTracking()
                .Where(p => p.TurmaId == turmaId && p.Data >= inicio && p.Data <= fim)
                .ToListAsync();
        }

        public async Task SalvarChamadaAsync(IEnumerable<Presenca> novasPresencas)
        {
            await _context.Set<Presenca>().AddRangeAsync(novasPresencas);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Aluno>> ObterAlunosEmReposicaoAsync(Guid turmaDestinoId, DateOnly data)
        {
            return await (from reposicao in _context.Reposicoes
                          join aluno in _context.Set<Aluno>() on reposicao.AlunoId equals aluno.Id
                          where reposicao.TurmaDestinoId == turmaDestinoId
                                && reposicao.DataReposicao == data
                                && aluno.Status != StatusAluno.Inativo
                          select aluno)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}