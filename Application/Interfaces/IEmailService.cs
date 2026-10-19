namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task EnviarAsync(string destinatario, string assunto, string corpoTexto);
    }
}
