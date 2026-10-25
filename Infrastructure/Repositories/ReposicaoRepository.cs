using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReposicaoRepository : IReposicaoRepository
{
    private readonly AppDbContext _context;

    public ReposicaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Reposicao>> ListarTodasAsync()
    {
        return await _context.Reposicoes.AsNoTracking().ToListAsync();
    }

    public async Task AdicionarAsync(Reposicao reposicao)
    {
        await _context.Reposicoes.AddAsync(reposicao);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteReposicaoNoMesmoDiaAsync(Guid alunoId, DateOnly dataOriginal)
    {
        return await _context.Reposicoes.AnyAsync(r => r.AlunoId == alunoId && r.DataOriginal == dataOriginal);
    }
}
