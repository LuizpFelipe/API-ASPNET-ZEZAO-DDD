using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICodigoRecuperacaoSenhaRepository
    {
        Task AdicionarAsync(CodigoRecuperacaoSenha codigo);
        Task<CodigoRecuperacaoSenha?> ObterMaisRecenteDoUsuarioAsync(Guid usuarioId);
        Task<CodigoRecuperacaoSenha?> ObterPorTokenHashAsync(string tokenHash);
        Task InvalidarPendentesDoUsuarioAsync(Guid usuarioId);
        Task AtualizarAsync(CodigoRecuperacaoSenha codigo);
    }
}