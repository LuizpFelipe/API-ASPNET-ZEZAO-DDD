namespace Domain.Entities
{
    public class CodigoRecuperacaoSenha
    {
        public Guid Id { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string CodigoHash { get; private set; } = string.Empty;
        public DateTime ExpiraEm { get; private set; }
        public int Tentativas { get; private set; }
        public bool Usado { get; private set; }
        public string? TokenHash { get; private set; }
        public DateTime? TokenExpiraEm { get; private set; }
        public DateTime CriadoEm { get; private set; }

        protected CodigoRecuperacaoSenha() { }

        public CodigoRecuperacaoSenha(Guid usuarioId, string codigoHash, DateTime expiraEm)
        {
            Id = Guid.NewGuid();
            UsuarioId = usuarioId;
            CodigoHash = codigoHash;
            ExpiraEm = expiraEm;
            CriadoEm = DateTime.UtcNow;
        }

        public bool PodeValidarCodigo(DateTime agora, int maximoDeTentativas)
        {
            return !Usado && TokenHash == null && ExpiraEm > agora && Tentativas < maximoDeTentativas;
        }

        public bool PodeRedefinirSenha(DateTime agora)
        {
            return !Usado && TokenHash != null && TokenExpiraEm.HasValue && TokenExpiraEm.Value > agora;
        }

        public void RegistrarTentativaInvalida()
        {
            Tentativas++;
        }

        public void MarcarCodigoValidado(string tokenHash, DateTime tokenExpiraEm)
        {
            TokenHash = tokenHash;
            TokenExpiraEm = tokenExpiraEm;
        }

        public void MarcarComoUsado()
        {
            Usado = true;
        }
    }
}