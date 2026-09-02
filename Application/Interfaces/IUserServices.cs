using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<UserResponseDTO> CreateUser(UserRequestDTO user);
    }
}
