using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICampeonatoCompeticaoRepository
    {
        Task<List<EquipeCampeonato>> ObterEquipesAsync(Guid campeonatoId);
        Task<EquipeCampeonato?> ObterEquipePorIdAsync(Guid equipeId);
        Task AdicionarEquipesAsync(IEnumerable<EquipeCampeonato> equipes, IEnumerable<ParticipacaoCampeonato> participacoesAtualizadas);
        Task RemoverEquipeAsync(EquipeCampeonato equipe, IEnumerable<ParticipacaoCampeonato> participacoesLiberadas);

        Task<List<ParticipacaoCampeonato>> ObterParticipacoesAsync(Guid campeonatoId);
        Task<ParticipacaoCampeonato?> ObterParticipacaoPorIdAsync(Guid participacaoId);
        Task RemoverParticipacaoAsync(ParticipacaoCampeonato participacao);

        Task<List<JogoCampeonato>> ObterJogosAsync(Guid campeonatoId);
        Task<JogoCampeonato?> ObterJogoPorIdAsync(Guid jogoId);
        Task AdicionarJogosAsync(IEnumerable<JogoCampeonato> jogos);
        Task AtualizarJogosAsync(IEnumerable<JogoCampeonato> jogos);
        Task RemoverJogosDoCampeonatoAsync(Guid campeonatoId);

        Task<List<EstatisticaJogo>> ObterEstatisticasDoJogoAsync(Guid jogoId);
        Task<List<EstatisticaJogo>> ObterEstatisticasDoCampeonatoAsync(Guid campeonatoId);
        Task SubstituirEstatisticasDoJogoAsync(Guid jogoId, IEnumerable<EstatisticaJogo> estatisticas);
    }
}
