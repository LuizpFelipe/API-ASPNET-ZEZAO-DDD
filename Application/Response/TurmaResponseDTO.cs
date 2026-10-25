namespace Application.Response
{
    public class TurmaResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }

        // O Data retornará um objeto anônimo ou tipado contendo: Id, Nome, DiasSemana, Horario, CategoriaNome, ProfessorNome
        public object Data { get; set; } = string.Empty;
    }
}