using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAvisoCobrancaRepository
    {
        Task AdicionarAsync(AvisoCobranca avisoCobranca);
        Task<IEnumerable<AvisoCobranca>> ObterPorParcelaAsync(Guid parcelaId);
    }
}