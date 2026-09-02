using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class UserService : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityService _securityService;

        public UserService(IUserRepository userRepository, ISecurityService securityService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
        }

        public async Task<UserResponseDTO> CreateUser(UserRequestDTO userRequestDTO)
        {
            try
            {
                if (userRequestDTO == null)
                {
                    return new UserResponseDTO
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                string passwordHash = _securityService.HashPassword(userRequestDTO.Password);

                var newUser = new User
                (
                    userRequestDTO.Name,
                    userRequestDTO.Email,
                    passwordHash
                );

                await _userRepository.AddAsync(newUser);

                return new UserResponseDTO
                {
                    Message = "User created successfully",
                    Status = "Success",
                    Data = new UserData
                    {
                        Name = newUser.Name,
                        Email = newUser.Email
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
        }
    }
}
