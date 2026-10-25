using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Request;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Coordenador")]
    public class TurmasController : ControllerBase
    {
        private readonly ITurmaService _turmaService;

        public TurmasController(ITurmaService turmaService)
        {
            _turmaService = turmaService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _turmaService.ListarAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _turmaService.ObterPorIdAsync(id);
            if (!result.Status) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("categoria/{categoriaId}")]
        public async Task<IActionResult> GetByCategoria(Guid categoriaId)
        {
            var result = await _turmaService.ListarPorCategoriaAsync(categoriaId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarTurmaRequestDTO dto)
        {
            var result = await _turmaService.CriarAsync(dto);
            if (!result.Status) return BadRequest(result); 
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AtualizarTurmaRequestDTO dto)
        {
            var result = await _turmaService.AtualizarAsync(id, dto);
            if (!result.Status) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _turmaService.RemoverAsync(id);
            if (!result.Status) return BadRequest(result);
            return Ok(result);
        }
    }
}