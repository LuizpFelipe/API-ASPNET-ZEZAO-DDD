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

        // Uso interno: o controller usa para escolher 400, 403 ou 404. Não vai no JSON.
        [JsonIgnore]
        public TipoFalhaPresenca Falha { get; set; }
    }

    public class ChamadaDataDTO
    {
        public Guid TurmaId { get; set; }
        public string TurmaNome { get; set; } = string.Empty;
        public DateOnly Data { get; set; }
        public List<ChamadaAlunoDTO> Alunos { get; set; } = new();
    }

    // Só dados técnicos e de presença. Nenhum valor financeiro, só a sinalização.
    public class ChamadaAlunoDTO
    {
        public Guid AlunoId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public string NivelTecnico { get; set; } = string.Empty;
        public bool EhInadimplente { get; set; }

        // null = ainda não há registro de presença para esse aluno nessa data.
        public bool? Presente { get; set; }
    }
}