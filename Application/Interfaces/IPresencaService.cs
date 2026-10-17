using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IPresencaService
    {
        Task<TurmaDoProfessorResponseDTO> ListarTurmasDoProfessorAsync(string nomeUsuario, bool ehCoordenador);

        Task<ChamadaResponseDTO> ObterChamadaAsync(Guid turmaId, DateOnly? data, string nomeUsuario, bool ehCoordenador);

        Task<ChamadaResponseDTO> RegistrarChamadaAsync(Guid turmaId, RegistrarPresencaRequestDTO dto, string nomeUsuario, bool ehCoordenador);
    }
}