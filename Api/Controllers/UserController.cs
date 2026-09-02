using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Example endpoint demonstrating the DDD layering (Api -> Application -> Domain -> Infrastructure)
    /// used by this template. Use it as a reference when adding new features.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserServices userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRequestDTO userRequestDTO)
        {
            try
            {
                var result = await _userService.CreateUser(userRequestDTO);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(StatusCodes.Status500InternalServerError, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while registering user");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
