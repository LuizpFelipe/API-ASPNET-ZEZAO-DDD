using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Interfaces;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISecurityService _securityService;
        private readonly ITokenService _tokenService;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            ISecurityService securityService,
            ITokenService tokenService)
        {
            _usuarioRepository = usuarioRepository;
            _securityService = securityService;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                if (loginRequestDTO == null
                    || string.IsNullOrWhiteSpace(loginRequestDTO.NomeUsuario)
                    || string.IsNullOrWhiteSpace(loginRequestDTO.Senha))
                {
                    return new LoginResponseDTO
                    {
                        Message = "Usuário e senha são obrigatórios.",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                var usuario = await _usuarioRepository.ObterPorNomeUsuarioAsync(loginRequestDTO.NomeUsuario);
                if (usuario == null || !_securityService.VerifyPassword(loginRequestDTO.Senha, usuario.SenhaHash))
                {
                    return new LoginResponseDTO
                    {
                        Message = "Usuário ou senha inválidos.",
                        Status = "unauthorized",
                        Data = null
                    };
                }

                var token = _tokenService.GenerateToken(usuario);

                return new LoginResponseDTO
                {
                    Message = "Login realizado com sucesso.",
                    Status = "Success",
                    Data = new LoginData
                    {
                        NomeUsuario = usuario.NomeUsuario,
                        Perfil = usuario.Perfil.ToString(),
                        Token = token
                    }
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
        }
    }
}