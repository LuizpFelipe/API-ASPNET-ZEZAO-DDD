using System.Security.Claims;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Roles = "Professor,Coordenador")]
    public class PresencaController : ControllerBase
    {
        private readonly IPresencaService _presencaService;
        private readonly ILogger<PresencaController> _logger;

        public PresencaController(IPresencaService presencaService, ILogger<PresencaController> logger)
        {
            _presencaService = presencaService;
            _logger = logger;
        }

        // Quem está logado vem do token, nunca de parâmetro da URL.
        private string NomeUsuarioLogado =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? string.Empty;

        private bool EhCoordenador => User.IsInRole("Coordenador");

        // GET /api/Professores/minhas-turmas
        [HttpGet("Professores/minhas-turmas")]
        public async Task<IActionResult> MinhasTurmas()
        {
            try
            {
                var response = await _presencaService.ListarTurmasDoProfessorAsync(NomeUsuarioLogado, EhCoordenador);
                if (!response.Status) return ResponderFalha(response.Falha, response);
                return Ok(response);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro ao listar as turmas do usuário {NomeUsuario}.", NomeUsuarioLogado);

                return StatusCode(StatusCodes.Status500InternalServerError, new TurmaDoProfessorResponseDTO
                {
                    Status = false,
                    Message = "Não foi possível carregar as turmas agora. Tente novamente em instantes. Se continuar, avise o suporte."
                });
            }
        }

        // GET /api/Turmas/{id}/chamada?data=2026-09-26
        [HttpGet("Turmas/{id:guid}/chamada")]
        public async Task<IActionResult> ObterChamada(Guid id, [FromQuery] DateOnly? data)
        {
            try
            {
                var response = await _presencaService.ObterChamadaAsync(id, data, NomeUsuarioLogado, EhCoordenador);
                if (!response.Status) return ResponderFalha(response.Falha, response);
                return Ok(response);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro ao carregar a chamada da turma {TurmaId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new ChamadaResponseDTO
                {
                    Status = false,
                    Message = "Não foi possível carregar a chamada agora. Tente novamente em instantes. Se continuar, avise o suporte."
                });
            }
        }

        // POST /api/Turmas/{id}/chamada
        [HttpPost("Turmas/{id:guid}/chamada")]
        public async Task<IActionResult> RegistrarChamada(Guid id, [FromBody] RegistrarPresencaRequestDTO dto)
        {
            try
            {
                var response = await _presencaService.RegistrarChamadaAsync(id, dto, NomeUsuarioLogado, EhCoordenador);
                if (!response.Status) return ResponderFalha(response.Falha, response);
                return Ok(response);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro ao salvar a chamada da turma {TurmaId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new ChamadaResponseDTO
                {
                    Status = false,
                    Message = "Não foi possível salvar a chamada agora. Confira a lista e tente de novo. Se continuar, avise o suporte."
                });
            }
        }

        private IActionResult ResponderFalha(TipoFalhaPresenca falha, object response)
        {
            return falha switch
            {
                TipoFalhaPresenca.NaoEncontrado => NotFound(response),
                TipoFalhaPresenca.AcessoNegado => StatusCode(StatusCodes.Status403Forbidden, response),
                _ => BadRequest(response)
            };
        }
    }
}