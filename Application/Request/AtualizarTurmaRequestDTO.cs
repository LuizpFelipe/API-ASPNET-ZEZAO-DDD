using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Request
{
    public class AtualizarTurmaRequestDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string DiasSemana { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public Guid CategoriaId { get; set; }
        public Guid ProfessorId { get; set; }
    }
}
