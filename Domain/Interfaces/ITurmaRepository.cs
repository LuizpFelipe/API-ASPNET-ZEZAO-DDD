using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITurmaRepository
    {
        Task<IEnumerable<Turma>> ObterTodasAsync();
        Task<Turma> ObterPorIdAsync(Guid id);
        Task<IEnumerable<Turma>> ObterPorCategoriaAsync(Guid categoriaId);
        Task<int> ContarAlunosPorTurmaAsync(Guid turmaId);
        Task AdicionarAsync(Turma turma);
        Task AtualizarAsync(Turma turma);
        Task RemoverAsync(Turma turma);
    }
}