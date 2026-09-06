using Application.Request;
using Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ITurmaService
    {
        Task<IEnumerable<TurmaResponseDTO>> ListarAsync();
        Task<TurmaResponseDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<TurmaResponseDTO>> ListarPorCategoriaAsync(Guid categoriaId);
        Task<TurmaResponseDTO> CriarAsync(CriarTurmaRequestDTO dto);
        Task<TurmaResponseDTO> AtualizarAsync(Guid id, AtualizarTurmaRequestDTO dto);
        Task<TurmaResponseDTO> RemoverAsync(Guid id);
    }
}