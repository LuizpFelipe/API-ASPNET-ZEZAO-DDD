using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
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
    }
}