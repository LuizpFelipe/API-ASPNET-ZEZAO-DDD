using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessoresController : ControllerBase
    {
        private readonly IProfessorService _professorService;

        public ProfessoresController(IProfessorService professorService)
        {
            _professorService = professorService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _professorService.ListarAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _professorService.ObterPorIdAsync(id);
            if (!result.Status) return NotFound(result);
            return Ok(result);
        }

        [Authorize(Roles = "Coordenador")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarProfessorRequestDTO dto)
        {
            var result = await _professorService.CriarAsync(dto);
            if (!result.Status) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Coordenador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AtualizarProfessorRequestDTO dto)
        {
            var result = await _professorService.AtualizarAsync(id, dto);
            if (!result.Status) return BadRequest(result);
            return Ok(result);
        }

        [Authorize(Roles = "Coordenador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _professorService.RemoverAsync(id);
            if (!result.Status) return BadRequest(result);
            return Ok(result);
        }
    }
}