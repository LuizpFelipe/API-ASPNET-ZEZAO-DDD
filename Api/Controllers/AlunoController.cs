using Api.Utilitarios;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlunosController : ControllerBase
{
    private const long LimiteDaRequisicaoEmBytes = 6 * 1024 * 1024;

    private readonly IAlunoService _alunoService;
    private readonly IArmazenamentoArquivoService _armazenamentoService;
    private readonly IPromocaoDeCategoriaService _promocaoDeCategoriaService;

    public AlunosController(
        IAlunoService alunoService,
        IArmazenamentoArquivoService armazenamentoService,
        IPromocaoDeCategoriaService promocaoDeCategoriaService)
    {
        _alunoService = alunoService;
        _armazenamentoService = armazenamentoService;
        _promocaoDeCategoriaService = promocaoDeCategoriaService;
    }

    /// <summary>Executa na hora a promoção de categoria por aniversário (a rotina diária faz o mesmo sozinha).</summary>
    [Authorize(Roles = "Coordenador")]
    [HttpPost("promover-categorias")]
    public async Task<IActionResult> PromoverCategorias()
    {
        var resultado = await _promocaoDeCategoriaService.ExecutarAsync();
        return Ok(new { Status = true, Message = "Promoção de categoria executada.", Data = resultado });
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

    [Authorize(Roles = "Coordenador")]
    [RequestSizeLimit(LimiteDaRequisicaoEmBytes)]
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] CriarAlunoRequestDTO dto, IFormFile? foto)
    {
        dto.FotoUrl = null;

        var erroDaFoto = ValidadorDeFoto.Validar(foto);
        if (erroDaFoto != null)
            return BadRequest(new AlunoResponseDTO { Status = false, Message = erroDaFoto });

        if (foto != null && foto.Length > 0)
        {
            using var stream = foto.OpenReadStream();
            dto.FotoUrl = await _armazenamentoService.SalvarAsync(stream, ValidadorDeFoto.NomeSeguro(foto));
        }

        var result = await _alunoService.CriarAsync(dto);
        if (!result.Status) return BadRequest(result);

        return Created("", result);
    }

    [Authorize(Roles = "Coordenador")]
    [RequestSizeLimit(LimiteDaRequisicaoEmBytes)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromForm] AtualizarAlunoRequestDTO dto, IFormFile? foto)
    {
        dto.FotoUrl = null;

        var erroDaFoto = ValidadorDeFoto.Validar(foto);
        if (erroDaFoto != null)
            return BadRequest(new AlunoResponseDTO { Status = false, Message = erroDaFoto });

        if (foto != null && foto.Length > 0)
        {
            using var stream = foto.OpenReadStream();
            dto.FotoUrl = await _armazenamentoService.SalvarAsync(stream, ValidadorDeFoto.NomeSeguro(foto));
        }

        var result = await _alunoService.AtualizarAsync(id, dto);
        if (!result.Status) return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "Coordenador")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _alunoService.RemoverAsync(id);
        if (!result.Status) return NotFound(result);

        return Ok(result);
    }
}