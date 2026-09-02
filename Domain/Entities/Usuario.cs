using Domain.Enums;

namespace Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; private set; }
        public string NomeUsuario { get; private set; } = string.Empty;
        public string SenhaHash { get; private set; } = string.Empty;
        public PerfilUsuario Perfil { get; private set; }

        public Usuario()
        {
        }

        public Usuario(string nomeUsuario, string senhaHash, PerfilUsuario perfil)
        {
            Id = Guid.NewGuid();
            NomeUsuario = nomeUsuario;
            SenhaHash = senhaHash;
            Perfil = perfil;
        }
    }
}
