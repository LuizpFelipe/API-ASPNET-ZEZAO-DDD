using Application.Interfaces;
using Application.Request;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Coordenador")] 
public class AlunosController : ControllerBase
{
    private readonly IAlunoService _alunoService;
    private readonly IArmazenamentoArquivoService _armazenamentoService;

    public AlunosController(IAlunoService alunoService, IArmazenamentoArquivoService armazenamentoService)
    {
        _alunoService = alunoService;
        _armazenamentoService = armazenamentoService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _alunoService.ListarAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _alunoService.ObterPorIdAsync(id);
        if (!result.Status) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm] CriarAlunoRequestDTO dto, IFormFile? foto)
    {
        if (foto != null && foto.Length > 0)
        {
            var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
            using var stream = foto.OpenReadStream();
            dto.FotoUrl = await _armazenamentoService.SalvarAsync(stream, nomeArquivo);
        }

        var result = await _alunoService.CriarAsync(dto);
        if (!result.Status) return BadRequest(result);

        return Created("", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromForm] AtualizarAlunoRequestDTO dto, IFormFile? foto)
    {
        if (foto != null && foto.Length > 0)
        {
            var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
            using var stream = foto.OpenReadStream();
            dto.FotoUrl = await _armazenamentoService.SalvarAsync(stream, nomeArquivo);
        }

        var result = await _alunoService.AtualizarAsync(id, dto);
        if (!result.Status) return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _alunoService.RemoverAsync(id);
        if (!result.Status) return NotFound(result);

        return Ok(result);
    }
}