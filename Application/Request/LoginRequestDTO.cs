namespace Application.Request
{
    public class LoginRequestDTO
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}