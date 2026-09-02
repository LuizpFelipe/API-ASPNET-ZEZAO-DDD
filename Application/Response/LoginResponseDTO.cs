namespace Application.Response
{
    public class LoginResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public LoginData? Data { get; set; }
    }

    public class LoginData
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}