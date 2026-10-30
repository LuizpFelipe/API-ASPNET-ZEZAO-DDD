namespace Application.Response
{
    public class ResultadoDaPromocaoDeCategoriaDTO
    {
        public int AlunosPromovidos { get; set; }

        /// <summary>Alunos que já deveriam subir, mas a categoria nova não tem nenhuma turma cadastrada.</summary>
        public List<string> AlunosSemTurmaNaNovaCategoria { get; set; } = new();
    }
}
