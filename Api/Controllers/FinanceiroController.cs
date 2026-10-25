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

        public FinanceiroController(IFinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
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
            return Ok(result);
        }

        [HttpPost("/api/Parcelas/{id}/cobranca")]
        public async Task<IActionResult> DispararCobranca(Guid id)
        {
            var result = await _financeiroService.DispararCobrancaAsync(id);
            if (!result.Status) return BadRequest(result);
            return Ok(result);
        }
    }
}