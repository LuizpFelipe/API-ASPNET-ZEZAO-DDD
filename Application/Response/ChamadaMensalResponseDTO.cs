using System.Text.Json.Serialization;

namespace Application.Response
{
    public class ChamadaMensalResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public ChamadaMensalDataDTO? Data { get; set; }

        [JsonIgnore]
        public TipoFalhaPresenca Falha { get; set; }
    }

    public class ChamadaMensalDataDTO
    {
        public ChamadaMensalTurmaDTO Turma { get; set; } = new();
        public string Mes { get; set; } = string.Empty;
        public List<ChamadaMensalDataDeAulaDTO> Datas { get; set; } = new();
        public List<ChamadaMensalAlunoDTO> Alunos { get; set; } = new();
    }

    public class ChamadaMensalTurmaDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string DiasSemana { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
    }

    public class ChamadaMensalDataDeAulaDTO
    {
        public DateOnly Data { get; set; }
        public bool PodeEditar { get; set; }
    }

    public class ChamadaMensalAlunoDTO
    {
        public Guid AlunoId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public bool EhInadimplente { get; set; }
        public List<ChamadaMensalPresencaDTO> Presencas { get; set; } = new();
    }

    public class ChamadaMensalPresencaDTO
    {
        public DateOnly Data { get; set; }

        // null = sem registro de presença nessa data.
        public bool? Presente { get; set; }
    }
}