using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface ICoordenadorService
    {
        Task<IEnumerable<CoordenadorDataDTO>> ListarAsync();
        Task<CoordenadorResponseDTO> ObterPorIdAsync(Guid id);
        Task<CoordenadorResponseDTO> CriarAsync(CriarCoordenadorRequestDTO dto);
        Task<CoordenadorResponseDTO> AtualizarAsync(Guid id, AtualizarCoordenadorRequestDTO dto);
        Task<CoordenadorResponseDTO> RemoverAsync(Guid id);
    }
}