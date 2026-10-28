using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public record ParcelaDetalhada(
        Parcela Parcela,
        Guid AlunoId,
        string AlunoNome,
        string ResponsavelNome,
        string Telefone,
        StatusContrato StatusContrato);

    public interface IParcelaRepository
    {
        Task AdicionarVariasAsync(IEnumerable<Parcela> parcelas);
        Task<IEnumerable<Parcela>> ObterComFiltrosAsync(Guid? alunoId, string? status, int? mes);
        Task<Parcela?> ObterPorIdAsync(Guid id);
        Task AtualizarAsync(Parcela parcela);
        Task<IEnumerable<Parcela>> ObterTodasAsync();
        Task<IEnumerable<ParcelaDetalhada>> ObterDetalhadasAsync(Guid? alunoId, string? status, int? mes, DateOnly hoje);
        Task<ParcelaDetalhada?> ObterDetalhadaPorIdAsync(Guid id);
    }
}