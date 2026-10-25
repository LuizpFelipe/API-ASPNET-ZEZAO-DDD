namespace Application.Response
{
    public class CoordenadorResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public CoordenadorDataDTO? Data { get; set; }
    }

    public class CoordenadorDataDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}