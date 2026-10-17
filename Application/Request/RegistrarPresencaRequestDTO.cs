namespace Application.Request
{
    public class RegistrarPresencaRequestDTO
    {
        public DateOnly Data { get; set; }
        public List<PresencaAlunoRequestDTO> Presencas { get; set; } = new();
    }

    public class PresencaAlunoRequestDTO
    {
        public Guid AlunoId { get; set; }
        public bool Presente { get; set; }
    }
}