using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface ICampeonatoCompeticaoService
    {
        Task<RespostaDaCompeticao<List<AlunoElegivelDTO>>> ListarElegiveisAsync(Guid campeonatoId);
        Task<RespostaDaCompeticao<List<EquipeDTO>>> ListarEquipesAsync(Guid campeonatoId);
        Task<RespostaDaCompeticao<EquipeDTO>> CriarEquipeAsync(Guid campeonatoId, CriarEquipeRequestDTO dto);
        Task<RespostaDaCompeticao<List<EquipeDTO>>> SortearEquipesAsync(Guid campeonatoId, SortearEquipesRequestDTO dto);
        Task<RespostaDaCompeticao<bool>> RemoverEquipeAsync(Guid campeonatoId, Guid equipeId);
        Task<RespostaDaCompeticao<bool>> RemoverParticipacaoAsync(Guid campeonatoId, Guid participacaoId);
        Task<RespostaDaCompeticao<List<JogoDTO>>> GerarJogosAsync(Guid campeonatoId);
        Task<RespostaDaCompeticao<List<JogoDTO>>> ListarJogosAsync(Guid campeonatoId);
        Task<RespostaDaCompeticao<bool>> ReiniciarJogosAsync(Guid campeonatoId);
        Task<RespostaDaCompeticao<EstatisticasDoJogoDTO>> ObterEstatisticasAsync(Guid jogoId);
        Task<RespostaDaCompeticao<EstatisticasDoJogoDTO>> SalvarEstatisticasAsync(Guid jogoId, SalvarEstatisticasRequestDTO dto);
        Task<RespostaDaCompeticao<EstatisticasDoJogoDTO>> FinalizarJogoAsync(Guid jogoId);
        Task<RespostaDaCompeticao<RankingDoCampeonatoDTO>> ObterRankingAsync(Guid campeonatoId);
    }
}
