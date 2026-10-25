using Domain.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task ExecutarEmTransacaoAsync(Func<Task> acao)
    {
        await using var transacao = await _context.Database.BeginTransactionAsync();
        await acao();
        await transacao.CommitAsync();
    }
}
