using System;

namespace Application.Request
{
    public class InscreverAlunoCampeonatoRequestDTO
    {
        public Guid AlunoId { get; set; }
        public string? Posicao { get; set; }
    }
}
