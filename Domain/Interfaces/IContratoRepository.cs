using Domain.Entities;

namespace Domain.Interfaces;

public interface IContratoRepository
{
    Task<IEnumerable<Contrato>> ObterTodosAsync();
    Task<Contrato?> ObterPorIdAsync(Guid id);
    Task AdicionarAsync(Contrato contrato);
    Task AtualizarAsync(Contrato contrato);
    Task<bool> ExisteContratoAtivoDoAlunoAsync(Guid alunoId);
}
