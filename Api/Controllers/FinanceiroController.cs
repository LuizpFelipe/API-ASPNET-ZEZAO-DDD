using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Coordenador")] // Restrito exclusivamente para a role Coordenador
    public class FinanceiroController : ControllerBase
    {
        private readonly IFinanceiroService _financeiroService;
        private readonly IRotinaFinanceiraService _rotinaFinanceiraService;

        public FinanceiroController(IFinanceiroService financeiroService, IRotinaFinanceiraService rotinaFinanceiraService)
        {
            _financeiroService = financeiroService;
            _rotinaFinanceiraService = rotinaFinanceiraService;
        }

        [HttpGet("resumo")]
        public async Task<IActionResult> ObterResumo()
        {
            var result = await _financeiroService.ObterResumoAsync();
            return Ok(result);
        }

        [HttpGet("/api/Parcelas")]
        public async Task<IActionResult> ListarParcelas([FromQuery] Guid? alunoId, [FromQuery] string? status, [FromQuery] int? mes)
        {
            var result = await _financeiroService.ListarParcelasAsync(alunoId, status, mes);
            return Ok(result);
        }

        [HttpPut("/api/Parcelas/{id}/pagamento")]
        public async Task<IActionResult> RegistrarPagamento(Guid id)
        {
            var result = await _financeiroService.RegistrarPagamentoAsync(id);
            if (!result.Status) return BadRequest(result);

            // O aluno que quitou a última parcela vencida volta a Ativo na hora, sem esperar a rotina.
            await _rotinaFinanceiraService.SincronizarStatusDosAlunosAsync();
            return Ok(result);
        }

        [HttpPost("/api/Parcelas/{id}/cobranca")]
        public async Task<IActionResult> DispararCobranca(Guid id)
        {
            var result = await _financeiroService.DispararCobrancaAsync(id);
            if (!result.Status) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("rotina")]
        public async Task<IActionResult> ExecutarRotina()
        {
            var resultado = await _rotinaFinanceiraService.ExecutarAsync();
            return Ok(new { Status = true, Message = "Rotina financeira executada.", Data = resultado });
        }
    }
}