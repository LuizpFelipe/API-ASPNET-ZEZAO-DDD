using Domain.Entities;

namespace Domain.Interfaces;

public interface IReposicaoRepository
{
    Task<IEnumerable<Reposicao>> ListarTodasAsync();
    Task AdicionarAsync(Reposicao reposicao);
    Task<bool> ExisteReposicaoNoMesmoDiaAsync(Guid alunoId, DateOnly dataOriginal);
}
