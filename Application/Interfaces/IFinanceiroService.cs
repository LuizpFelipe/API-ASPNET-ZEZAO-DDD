using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Response;

namespace Application.Interfaces
{
    public interface IFinanceiroService
    {
        Task<IEnumerable<object>> ListarParcelasAsync(Guid? alunoId, string? status, int? mes);
        Task<ParcelaResponseDTO> RegistrarPagamentoAsync(Guid parcelaId);
        Task<ParcelaResponseDTO> DispararCobrancaAsync(Guid parcelaId);
        Task<ResumoFinanceiroResponseDTO> ObterResumoAsync();
    }
}