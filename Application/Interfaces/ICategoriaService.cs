using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDataDTO>> ListarAsync();
        Task<CategoriaResponseDTO> ObterPorIdAsync(Guid id);
        Task<CategoriaResponseDTO> CriarAsync(CriarCategoriaRequestDTO dto);
        Task<CategoriaResponseDTO> AtualizarAsync(Guid id, AtualizarCategoriaRequestDTO dto);
        Task<CategoriaResponseDTO> RemoverAsync(Guid id);
        Task<CategoriaDataDTO?> CalcularCategoriaPorIdadeAsync(DateOnly dataNascimento);
    }
}