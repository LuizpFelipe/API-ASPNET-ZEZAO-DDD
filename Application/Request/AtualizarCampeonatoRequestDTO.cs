using System;

namespace Application.Request
{
    public class AtualizarCampeonatoRequestDTO
    {
        public string Nome { get; set; } = string.Empty;
        public DateOnly Data { get; set; }
        public Guid CategoriaId { get; set; }
    }
}
