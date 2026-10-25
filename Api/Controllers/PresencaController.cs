using Application.Interfaces;
using Application.Request;
using Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        private string NomeUsuarioLogado =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? string.Empty;

        private bool EhCoordenador => User.IsInRole("Coordenador");

        [HttpGet("Professores/minhas-turmas")]
        public async Task<IActionResult> MinhasTurmas([FromQuery] DateOnly? data, [FromQuery] Guid? professorId)
        {
            try
            {
                var response = await _presencaService.ListarTurmasDoProfessorAsync(NomeUsuarioLogado, EhCoordenador, data, professorId);
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

        [HttpGet("Turmas/{id:guid}/chamada-mensal")]
        public async Task<IActionResult> ObterChamadaMensal(Guid id, [FromQuery] string? mes)
        {
            try
            {
                var response = await _presencaService.ObterChamadaMensalAsync(id, mes, NomeUsuarioLogado, EhCoordenador);
                if (!response.Status) return ResponderFalha(response.Falha, response);
                return Ok(response);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro ao carregar a chamada do mês da turma {TurmaId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new ChamadaMensalResponseDTO
                {
                    Status = false,
                    Message = "Não foi possível carregar a chamada do mês agora. Tente novamente em instantes. Se continuar, avise o suporte."
                });
            }
        }

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