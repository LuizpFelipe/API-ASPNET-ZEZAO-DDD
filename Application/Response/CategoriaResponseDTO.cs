using System;

namespace Application.Response
{
    public class CategoriaResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public CategoriaDataDTO? Data { get; set; }
    }

    public class CategoriaDataDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int IdadeMin { get; set; }
        public int IdadeMax { get; set; }
    }
}