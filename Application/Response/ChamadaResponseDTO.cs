using System.Text.Json.Serialization;

namespace Application.Response
{
    public enum TipoFalhaPresenca
    {
        Nenhuma,
        Invalido,
        NaoEncontrado,
        AcessoNegado
    }

    public class ChamadaResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public ChamadaDataDTO? Data { get; set; }

        [JsonIgnore]
        public TipoFalhaPresenca Falha { get; set; }
    }

    public class ChamadaDataDTO
    {
        public Guid TurmaId { get; set; }
        public string TurmaNome { get; set; } = string.Empty;
        public DateOnly Data { get; set; }
        public bool PodeEditar { get; set; }
        public string? MotivoSomenteLeitura { get; set; }
        public List<ChamadaAlunoDTO> Alunos { get; set; } = new();
    }

    public class ChamadaAlunoDTO
    {
        public Guid AlunoId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public string NivelTecnico { get; set; } = string.Empty;
        public bool EhInadimplente { get; set; }
        public bool? Presente { get; set; }
        public bool EhReposicao { get; set; }
    }
}