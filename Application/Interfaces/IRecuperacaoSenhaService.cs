using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IRecuperacaoSenhaService
    {
        Task<RecuperacaoSenhaResponseDTO> SolicitarCodigoAsync(SolicitarRecuperacaoSenhaRequestDTO dto);
        Task<RecuperacaoSenhaResponseDTO> ValidarCodigoAsync(ValidarCodigoRecuperacaoRequestDTO dto);
        Task<RecuperacaoSenhaResponseDTO> RedefinirSenhaAsync(RedefinirSenhaRequestDTO dto);
    }
}