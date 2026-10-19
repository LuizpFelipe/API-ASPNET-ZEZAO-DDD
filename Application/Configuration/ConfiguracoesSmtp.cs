namespace Application.Configuration
{
    public class ConfiguracoesSmtp
    {
        public string Servidor { get; set; } = string.Empty;
        public int Porta { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string EmailRemetente { get; set; } = string.Empty;
        public string NomeRemetente { get; set; } = string.Empty;
    }
}
