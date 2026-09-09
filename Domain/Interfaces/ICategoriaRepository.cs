using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> ObterTodasAsync();
        Task<Categoria?> ObterPorIdAsync(Guid id);
        Task<Categoria?> ObterPorIdadeAsync(int idade);
        Task<bool> ExisteSobreposicaoDeIdadeAsync(int idadeMin, int idadeMax, Guid? ignorarId = null);
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
        Task RemoverAsync(Categoria categoria);
    }
}