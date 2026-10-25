using Application.Configuration;
using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly ConfiguracoesSmtp _configuracoes;

        public EmailService(IOptions<ConfiguracoesSmtp> configuracoes)
        {
            _configuracoes = configuracoes.Value;
        }

        public async Task EnviarAsync(string destinatario, string assunto, string corpoTexto)
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(_configuracoes.NomeRemetente, _configuracoes.EmailRemetente));
            mensagem.To.Add(MailboxAddress.Parse(destinatario));
            mensagem.Subject = assunto;
            mensagem.Body = new TextPart("plain") { Text = corpoTexto };

            using var clienteSmtp = new SmtpClient();
            await clienteSmtp.ConnectAsync(
                _configuracoes.Servidor,
                _configuracoes.Porta,
                SecureSocketOptions.StartTlsWhenAvailable);

            if (!string.IsNullOrEmpty(_configuracoes.Usuario))
            {
                await clienteSmtp.AuthenticateAsync(_configuracoes.Usuario, _configuracoes.Senha);
            }

            await clienteSmtp.SendAsync(mensagem);
            await clienteSmtp.DisconnectAsync(true);
        }
    }
}
