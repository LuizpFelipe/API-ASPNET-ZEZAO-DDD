using Domain.Entities;

namespace Domain.Interfaces;

public interface IAlunoRepository
{
    Task<IEnumerable<Aluno>> ObterTodosAsync();
    Task<Aluno?> ObterPorIdAsync(Guid id);
    Task<bool> ExisteComMesmoNomeEDataNascimentoAsync(string nome, DateOnly dataNascimento, Guid? ignorarId = null);
    Task AdicionarAsync(Aluno aluno);
    Task AtualizarAsync(Aluno aluno);
}