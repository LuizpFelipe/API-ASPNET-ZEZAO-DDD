using Application.Interfaces;
using Application.Request;
using Application.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CompeticaoController : ControllerBase
    {
        private readonly ICampeonatoCompeticaoService _service;

        public CompeticaoController(ICampeonatoCompeticaoService service)
        {
            _service = service;
        }

        [HttpGet("Campeonatos/{id:guid}/elegiveis")]
        public async Task<IActionResult> GetElegiveis(Guid id) => Resposta(await _service.ListarElegiveisAsync(id));

        [HttpGet("Campeonatos/{id:guid}/equipes")]
        public async Task<IActionResult> GetEquipes(Guid id) => Resposta(await _service.ListarEquipesAsync(id));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpPost("Campeonatos/{id:guid}/equipes")]
        public async Task<IActionResult> PostEquipe(Guid id, [FromBody] CriarEquipeRequestDTO dto) =>
            Resposta(await _service.CriarEquipeAsync(id, dto));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpPost("Campeonatos/{id:guid}/equipes/aleatorias")]
        public async Task<IActionResult> PostEquipesAleatorias(Guid id, [FromBody] SortearEquipesRequestDTO dto) =>
            Resposta(await _service.SortearEquipesAsync(id, dto));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpDelete("Campeonatos/{id:guid}/equipes/{equipeId:guid}")]
        public async Task<IActionResult> DeleteEquipe(Guid id, Guid equipeId) =>
            Resposta(await _service.RemoverEquipeAsync(id, equipeId));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpDelete("Campeonatos/{id:guid}/participacoes/{participacaoId:guid}")]
        public async Task<IActionResult> DeleteParticipacao(Guid id, Guid participacaoId) =>
            Resposta(await _service.RemoverParticipacaoAsync(id, participacaoId));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpPost("Campeonatos/{id:guid}/jogos/gerar")]
        public async Task<IActionResult> PostGerarJogos(Guid id) => Resposta(await _service.GerarJogosAsync(id));

        [HttpGet("Campeonatos/{id:guid}/jogos")]
        public async Task<IActionResult> GetJogos(Guid id) => Resposta(await _service.ListarJogosAsync(id));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpDelete("Campeonatos/{id:guid}/jogos")]
        public async Task<IActionResult> DeleteJogos(Guid id) => Resposta(await _service.ReiniciarJogosAsync(id));

        [HttpGet("Jogos/{jogoId:guid}/estatisticas")]
        public async Task<IActionResult> GetEstatisticas(Guid jogoId) => Resposta(await _service.ObterEstatisticasAsync(jogoId));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpPut("Jogos/{jogoId:guid}/estatisticas")]
        public async Task<IActionResult> PutEstatisticas(Guid jogoId, [FromBody] SalvarEstatisticasRequestDTO dto) =>
            Resposta(await _service.SalvarEstatisticasAsync(jogoId, dto));

        [Authorize(Roles = "Coordenador,Professor")]
        [HttpPost("Jogos/{jogoId:guid}/finalizar")]
        public async Task<IActionResult> PostFinalizar(Guid jogoId) => Resposta(await _service.FinalizarJogoAsync(jogoId));

        [HttpGet("Campeonatos/{id:guid}/ranking")]
        public async Task<IActionResult> GetRanking(Guid id) => Resposta(await _service.ObterRankingAsync(id));

        private IActionResult Resposta<T>(RespostaDaCompeticao<T> resposta)
        {
            if (resposta.Status) return Ok(resposta);
            if (resposta.NaoEncontrado) return NotFound(resposta);
            return BadRequest(resposta);
        }
    }
}
