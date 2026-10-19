using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProfessorRepository
    {
        Task<IEnumerable<Professor>> ObterTodosAsync();
        Task<Professor?> ObterPorIdAsync(Guid id);
        Task AdicionarAsync(Professor professor);
        Task AtualizarAsync(Professor professor);
        Task RemoverAsync(Professor professor);
        Task<bool> PossuiTurmasAsync(Guid professorId);
    }
}
