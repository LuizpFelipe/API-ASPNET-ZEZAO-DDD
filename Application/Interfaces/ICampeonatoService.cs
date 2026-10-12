using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface ICampeonatoService
    {
        Task<IEnumerable<CampeonatoDataDTO>> ListarAsync();
        Task<CampeonatoResponseDTO> ObterPorIdAsync(Guid id);
        Task<CampeonatoResponseDTO> CriarAsync(CriarCampeonatoRequestDTO dto);
        Task<CampeonatoResponseDTO> AtualizarAsync(Guid id, AtualizarCampeonatoRequestDTO dto);
        Task<CampeonatoResponseDTO> RemoverAsync(Guid id);
        Task<ParticipacaoCampeonatoResponseDTO> InscreverAlunoAsync(Guid campeonatoId, InscreverAlunoCampeonatoRequestDTO dto);
        Task<ParticipacaoCampeonatoResponseDTO> AtualizarParticipacaoAsync(Guid id, AtualizarParticipacaoCampeonatoRequestDTO dto);
        Task<ParticipacaoCampeonatoResponseDTO> ListarParticipacoesAsync(Guid campeonatoId);
    }
}
