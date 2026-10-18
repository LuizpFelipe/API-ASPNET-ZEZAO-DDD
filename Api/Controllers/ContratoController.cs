using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Coordenador")]
public class ContratosController : ControllerBase
{
    private readonly IContratoService _contratoService;

    public ContratosController(IContratoService contratoService)
    {
        _contratoService = contratoService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _contratoService.ListarAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var result = await _contratoService.GerarPdfAsync(id);
        if (!result.Status) return NotFound(result);

        return File((byte[])result.Data!, "application/pdf", $"contrato-{id}.pdf");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] AtualizarContratoRequestDTO dto)
    {
        var result = await _contratoService.AtualizarAsync(id, dto);
        if (!result.Status) return BadRequest(result);

        return Ok(result);
    }
}
