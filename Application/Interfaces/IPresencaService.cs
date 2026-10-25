using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IPresencaService
    {

        Task<ChamadaResponseDTO> ObterChamadaAsync(Guid turmaId, DateOnly? data, string nomeUsuario, bool ehCoordenador);

        Task<ChamadaResponseDTO> RegistrarChamadaAsync(Guid turmaId, RegistrarPresencaRequestDTO dto, string nomeUsuario, bool ehCoordenador);

        Task<TurmaDoProfessorResponseDTO> ListarTurmasDoProfessorAsync(string nomeUsuario, bool ehCoordenador, DateOnly? data = null, Guid? professorIdFiltro = null);

        Task<ChamadaMensalResponseDTO> ObterChamadaMensalAsync(Guid turmaId, string? mes, string nomeUsuario, bool ehCoordenador);
    }
}