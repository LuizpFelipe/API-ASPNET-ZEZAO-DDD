using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/coordenadores")]
    [Authorize(Roles = "Coordenador")]
    public class CoordenadorController : ControllerBase
    {
        private readonly ICoordenadorService _coordenadorService;

        public CoordenadorController(ICoordenadorService coordenadorService)
        {
            _coordenadorService = coordenadorService;
        }

        // GET /api/coordenadores
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var coordenadores = await _coordenadorService.ListarAsync();
            return Ok(coordenadores);
        }

        // GET /api/coordenadores/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _coordenadorService.ObterPorIdAsync(id);
            if (!response.Status) return NotFound(response);
            return Ok(response);
        }

        // POST /api/coordenadores
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarCoordenadorRequestDTO dto)
        {
            var response = await _coordenadorService.CriarAsync(dto);
            if (!response.Status) return BadRequest(response);
            return CreatedAtAction(nameof(Get), new { id = response.Data?.Id }, response);
        }

        // PUT /api/coordenadores/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AtualizarCoordenadorRequestDTO dto)
        {
            var response = await _coordenadorService.AtualizarAsync(id, dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        // DELETE /api/coordenadores/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _coordenadorService.RemoverAsync(id);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }
    }
}