using Domain.Enums;

namespace Application.Request;

public class AtualizarAlunoRequestDTO
{
    public string NomeCompleto { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public NivelTecnico NivelTecnico { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public Guid TurmaId { get; set; }
    public string? FotoUrl { get; set; }
}