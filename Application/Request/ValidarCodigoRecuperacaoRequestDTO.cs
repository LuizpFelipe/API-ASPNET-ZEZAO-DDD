namespace Application.Request
{
    public class ValidarCodigoRecuperacaoRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }
}