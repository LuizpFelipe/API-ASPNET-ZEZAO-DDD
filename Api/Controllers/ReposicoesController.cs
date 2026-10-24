using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Coordenador")]
public class ReposicoesController : ControllerBase
{
    private readonly IReposicaoService _reposicaoService;

    public ReposicoesController(IReposicaoService reposicaoService)
    {
        _reposicaoService = reposicaoService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _reposicaoService.ListarAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RegistrarReposicaoRequestDTO dto)
    {
        var result = await _reposicaoService.RegistrarAsync(dto);
        if (!result.Status) return BadRequest(result);

        return Ok(result);
    }
}
