using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDTO> ResetarSenhaAsync(Guid usuarioId, ResetarSenhaUsuarioRequestDTO dto);
    }
}