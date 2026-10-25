
using Application.Request;
using Application.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProfessorService
    {
        Task<IEnumerable<ProfessorResponseDTO>> ListarAsync();
        Task<ProfessorResponseDTO> ObterPorIdAsync(Guid id);
        Task<ProfessorResponseDTO> CriarAsync(CriarProfessorRequestDTO dto);
        Task<ProfessorResponseDTO> AtualizarAsync(Guid id, AtualizarProfessorRequestDTO dto);
        Task<ProfessorResponseDTO> RemoverAsync(Guid id);
    }
}