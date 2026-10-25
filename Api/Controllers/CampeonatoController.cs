using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampeonatosController : ControllerBase
    {
        private readonly ICampeonatoService _campeonatoService;

        public CampeonatosController(ICampeonatoService campeonatoService)
        {
            _campeonatoService = campeonatoService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var campeonatos = await _campeonatoService.ListarAsync();
            return Ok(campeonatos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _campeonatoService.ObterPorIdAsync(id);
            if (!response.Status) return NotFound(response);
            return Ok(response);
        }

        [Authorize(Roles = "Coordenador")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarCampeonatoRequestDTO dto)
        {
            var response = await _campeonatoService.CriarAsync(dto);
            if (!response.Status) return BadRequest(response);
            return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
        }

        [Authorize(Roles = "Coordenador")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AtualizarCampeonatoRequestDTO dto)
        {
            var response = await _campeonatoService.AtualizarAsync(id, dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [Authorize(Roles = "Coordenador")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _campeonatoService.RemoverAsync(id);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("{id:guid}/participacoes")]
        public async Task<IActionResult> PostParticipacao(Guid id, [FromBody] InscreverAlunoCampeonatoRequestDTO dto)
        {
            var response = await _campeonatoService.InscreverAlunoAsync(id, dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("{id:guid}/participacoes")]
        public async Task<IActionResult> GetParticipacoes(Guid id)
        {
            var response = await _campeonatoService.ListarParticipacoesAsync(id);
            if (!response.Status) return NotFound(response);
            return Ok(response);
        }

        [HttpPut("~/api/Participacoes/{id:guid}")]
        public async Task<IActionResult> PutParticipacao(Guid id, [FromBody] AtualizarParticipacaoCampeonatoRequestDTO dto)
        {
            var response = await _campeonatoService.AtualizarParticipacaoAsync(id, dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }
    }
}
