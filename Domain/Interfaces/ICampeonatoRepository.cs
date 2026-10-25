using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICampeonatoRepository
    {
        Task<IEnumerable<Campeonato>> ObterTodosAsync();
        Task<Campeonato?> ObterPorIdAsync(Guid id);
        Task AdicionarAsync(Campeonato campeonato);
        Task AtualizarAsync(Campeonato campeonato);
        Task RemoverAsync(Campeonato campeonato);
        Task AdicionarParticipacaoAsync(ParticipacaoCampeonato participacao);
        Task<ParticipacaoCampeonato?> ObterParticipacaoPorIdAsync(Guid id);
        Task AtualizarParticipacaoAsync(ParticipacaoCampeonato participacao);
        Task<IEnumerable<ParticipacaoCampeonato>> ObterParticipacoesPorCampeonatoAsync(Guid campeonatoId);
    }
}
