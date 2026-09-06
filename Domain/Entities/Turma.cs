using System;

namespace Domain.Entities
{
    public class Turma
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string DiasSemana { get; private set; }
        public string Horario { get; private set; }
        public Guid CategoriaId { get; private set; }
        public Guid ProfessorId { get; private set; }

        public Turma(string nome, string diasSemana, string horario, Guid categoriaId, Guid professorId)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            DiasSemana = diasSemana;
            Horario = horario;
            CategoriaId = categoriaId;
            ProfessorId = professorId;
        }

        public void AtualizarDetalhes(string nome, string diasSemana, string horario, Guid categoriaId, Guid professorId)
        {
            Nome = nome;
            DiasSemana = diasSemana;
            Horario = horario;
            CategoriaId = categoriaId;
            ProfessorId = professorId;
        }
    }
}