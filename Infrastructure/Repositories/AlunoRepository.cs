using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AlunoRepository : IAlunoRepository
{
    private readonly AppDbContext _context; 

    public AlunoRepository(AppDbContext context) 
    {
        _context = context;
    }

    public async Task<IEnumerable<Aluno>> ObterTodosAsync()
    {
        return await _context.Set<Aluno>()
            .Where(a => a.Status != StatusAluno.Inativo)
            .ToListAsync();
    }

    public async Task<Aluno?> ObterPorIdAsync(Guid id)
    {
        return await _context.Set<Aluno>()
            .FirstOrDefaultAsync(a => a.Id == id && a.Status != StatusAluno.Inativo);
    }

    public async Task<bool> ExisteComMesmoNomeEDataNascimentoAsync(string nome, DateOnly dataNascimento, Guid? ignorarId = null)
    {
        var query = _context.Set<Aluno>().Where(a => a.NomeCompleto == nome && a.DataNascimento == dataNascimento && a.Status != StatusAluno.Inativo);
        if (ignorarId.HasValue)
        {
            query = query.Where(a => a.Id != ignorarId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task AdicionarAsync(Aluno aluno)
    {
        await _context.Set<Aluno>().AddAsync(aluno);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Aluno aluno)
    {
        _context.Set<Aluno>().Update(aluno);
        await _context.SaveChangesAsync();
    }
}