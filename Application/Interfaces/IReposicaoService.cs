using Application.Request;
using Application.Response;

namespace Application.Interfaces;

public interface IReposicaoService
{
    Task<ReposicaoResponseDTO> ListarAsync();
    Task<ReposicaoResponseDTO> RegistrarAsync(RegistrarReposicaoRequestDTO dto);
}
