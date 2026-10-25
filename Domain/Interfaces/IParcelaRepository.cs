using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IParcelaRepository
    {
        Task AdicionarVariasAsync(IEnumerable<Parcela> parcelas);
        Task<IEnumerable<Parcela>> ObterComFiltrosAsync(Guid? alunoId, string? status, int? mes);
        Task<Parcela?> ObterPorIdAsync(Guid id);
        Task AtualizarAsync(Parcela parcela);
        Task<IEnumerable<Parcela>> ObterTodasAsync();
    }
}