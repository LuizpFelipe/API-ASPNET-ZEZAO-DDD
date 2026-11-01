namespace Application.Request
{
    public class InativarTurmaRequestDTO
    {
        /// <summary>Turma que vai receber os alunos ativos. Obrigatória só quando a turma ainda tem alunos ativos.</summary>
        public Guid? TurmaDestinoId { get; set; }
    }
}
