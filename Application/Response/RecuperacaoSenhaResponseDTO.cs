namespace Application.Response
{
    public class RecuperacaoSenhaResponseDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public TokenRedefinicaoDataDTO? Data { get; set; }
    }

    public class TokenRedefinicaoDataDTO
    {
        public string Token { get; set; } = string.Empty;
    }
}