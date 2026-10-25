using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Coordenador")]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var categorias = await _categoriaService.ListarAsync();
            return Ok(categorias);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _categoriaService.ObterPorIdAsync(id);
            if (!response.Status) return NotFound(response);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarCategoriaRequestDTO dto)
        {
            var response = await _categoriaService.CriarAsync(dto);
            if (!response.Status) return BadRequest(response);
            return CreatedAtAction(nameof(Get), new { id = response.Data?.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AtualizarCategoriaRequestDTO dto)
        {
            var response = await _categoriaService.AtualizarAsync(id, dto);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _categoriaService.RemoverAsync(id);
            if (!response.Status) return BadRequest(response);
            return Ok(response);
        }
    }
}