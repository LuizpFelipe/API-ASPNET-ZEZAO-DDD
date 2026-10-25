using Application.Request;
using Application.Response;

namespace Application.Interfaces;

public interface IContratoService
{
    Task<ContratoResponseDTO> CriarAsync(Guid alunoId);
    Task<ContratoResponseDTO> ListarAsync();
    Task<ContratoResponseDTO> ObterPorIdAsync(Guid id);
    Task<ContratoResponseDTO> AtualizarAsync(Guid id, AtualizarContratoRequestDTO dto);
    Task<ContratoResponseDTO> GerarPdfAsync(Guid id);
}
