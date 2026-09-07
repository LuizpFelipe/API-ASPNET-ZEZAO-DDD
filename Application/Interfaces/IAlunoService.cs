using Application.Request;
using Application.Response;

namespace Application.Interfaces;

public interface IAlunoService
{
    Task<AlunoResponseDTO> ListarAsync();
    Task<AlunoResponseDTO> ObterPorIdAsync(Guid id);
    Task<AlunoResponseDTO> CriarAsync(CriarAlunoRequestDTO dto);
    Task<AlunoResponseDTO> AtualizarAsync(Guid id, AtualizarAlunoRequestDTO dto);
    Task<AlunoResponseDTO> RemoverAsync(Guid id);
}