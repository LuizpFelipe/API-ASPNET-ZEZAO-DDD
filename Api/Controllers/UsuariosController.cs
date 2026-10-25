using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Coordenador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPut("{id:guid}/resetar-senha")]
        public async Task<IActionResult> ResetarSenha(Guid id, [FromBody] ResetarSenhaUsuarioRequestDTO dto)
        {
            var response = await _usuarioService.ResetarSenhaAsync(id, dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }
    }
}