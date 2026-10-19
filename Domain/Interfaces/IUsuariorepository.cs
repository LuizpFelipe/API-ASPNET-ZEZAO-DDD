using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario);
        Task<Usuario> AddAsync(Usuario usuario);
        Task<Usuario?> ObterPorIdAsync(Guid id);
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<IEnumerable<Usuario>> ObterPorIdsAsync(IEnumerable<Guid> ids);
        Task AtualizarAsync(Usuario usuario);
    }
}
