using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICoordenadorRepository
    {
        Task<IEnumerable<Coordenador>> ObterTodosAsync();
        Task<Coordenador?> ObterPorIdAsync(Guid id);
        Task AdicionarAsync(Coordenador coordenador);
        Task AtualizarAsync(Coordenador coordenador);
        Task RemoverAsync(Coordenador coordenador);
        Task<bool> EmailJaCadastradoAsync(string email, Guid? ignorarUsuarioId = null);
    }
}