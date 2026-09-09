using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
    }
}