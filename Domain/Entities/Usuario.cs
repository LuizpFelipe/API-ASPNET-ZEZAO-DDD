using Domain.Enums;

namespace Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; private set; }
        public string NomeUsuario { get; private set; } = string.Empty;
        public string SenhaHash { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public PerfilUsuario Perfil { get; private set; }

        public Usuario()
        {
        }

        public Usuario(string nomeUsuario, string email, string senhaHash, PerfilUsuario perfil)
        {
            Id = Guid.NewGuid();
            NomeUsuario = nomeUsuario;
            Email = NormalizarEmail(email);
            SenhaHash = senhaHash;
            Perfil = perfil;
        }

        public void AlterarEmail(string novoEmail)
        {
            Email = NormalizarEmail(novoEmail);
        }

        public static bool TentarNormalizarEmail(string? email, out string emailNormalizado)
        {
            emailNormalizado = (email ?? string.Empty).Trim().ToLowerInvariant();

            return System.Net.Mail.MailAddress.TryCreate(emailNormalizado, out var enderecoDeEmail)
                   && enderecoDeEmail.Address == emailNormalizado;
        }

        private static string NormalizarEmail(string email)
        {
            if (!TentarNormalizarEmail(email, out var emailNormalizado))
            {
                throw new ArgumentException("E-mail inválido.", nameof(email));
            }

            return emailNormalizado;
        }
    }
}