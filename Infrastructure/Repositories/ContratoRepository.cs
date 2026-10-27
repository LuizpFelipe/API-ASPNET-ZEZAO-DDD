using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ContratoRepository : IContratoRepository
{
    private readonly AppDbContext _context;

    public ContratoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contrato>> ObterTodosAsync()
    {
        return await _context.Contratos.AsNoTracking().ToListAsync();
    }

    public async Task<Contrato?> ObterPorIdAsync(Guid id)
    {
        return await _context.Contratos.FindAsync(id);
    }

    public async Task AdicionarAsync(Contrato contrato)
    {
        await _context.Contratos.AddAsync(contrato);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Contrato contrato)
    {
        _context.Contratos.Update(contrato);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteContratoAtivoDoAlunoAsync(Guid alunoId)
    {
        return await _context.Contratos.AnyAsync(contrato => contrato.AlunoId == alunoId && contrato.Status == StatusContrato.Ativo);
    }
}
