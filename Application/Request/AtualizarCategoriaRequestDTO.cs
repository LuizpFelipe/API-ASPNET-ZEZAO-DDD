namespace Application.Request
{
    public class AtualizarCategoriaRequestDTO
    {
        public string Nome { get; set; } = string.Empty;
        public int IdadeMin { get; set; }
        public int IdadeMax { get; set; }
    }
}