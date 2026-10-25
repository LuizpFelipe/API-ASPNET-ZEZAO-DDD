using System.Text.Json.Serialization;

namespace Application.Response
{
    public class TurmaDoProfessorResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public List<TurmaDoProfessorDataDTO> Data { get; set; } = new();


        [JsonIgnore]
        public TipoFalhaPresenca Falha { get; set; }
    }

    public class TurmaDoProfessorDataDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string DiasSemana { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public Guid CategoriaId { get; set; }
        public Guid ProfessorId { get; set; }
        public bool? TemAulaNaData { get; set; }
    }
}