using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario);
        Task<Usuario> AddAsync(Usuario usuario);
    }
}
