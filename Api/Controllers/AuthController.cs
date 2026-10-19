using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRecuperacaoSenhaService _recuperacaoSenhaService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IRecuperacaoSenhaService recuperacaoSenhaService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _recuperacaoSenhaService = recuperacaoSenhaService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var result = await _authService.Login(loginRequestDTO);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "unauthorized" => Unauthorized(result),
                    "error" => StatusCode(StatusCodes.Status500InternalServerError, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while logging in");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AllowAnonymous]
        [HttpPost("recuperar-senha")]
        public async Task<IActionResult> RecuperarSenha([FromBody] SolicitarRecuperacaoSenhaRequestDTO dto)
        {
            var response = await _recuperacaoSenhaService.SolicitarCodigoAsync(dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("validar-codigo")]
        public async Task<IActionResult> ValidarCodigo([FromBody] ValidarCodigoRecuperacaoRequestDTO dto)
        {
            var response = await _recuperacaoSenhaService.ValidarCodigoAsync(dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("redefinir-senha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequestDTO dto)
        {
            var response = await _recuperacaoSenhaService.RedefinirSenhaAsync(dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }
    }
}