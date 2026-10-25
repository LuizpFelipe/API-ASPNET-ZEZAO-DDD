using System;

namespace Application.Response
{
    public class CampeonatoResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public CampeonatoDataDTO? Data { get; set; }
    }

    public class CampeonatoDataDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateOnly Data { get; set; }
        public Guid CategoriaId { get; set; }
    }
}
