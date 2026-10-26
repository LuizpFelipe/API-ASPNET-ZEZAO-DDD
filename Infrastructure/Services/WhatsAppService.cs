using System;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        public async Task<bool> EnviarMensagemAsync(string numeroTelefone, string mensagem)
        {
            // Exibe no terminal/console do Visual Studio ou VS Code para você visualizar o envio
            Console.WriteLine("========================================");
            Console.WriteLine("[WHATSAPP SIMULATOR] Enviando mensagem...");
            Console.WriteLine($"Para: {numeroTelefone}");
            Console.WriteLine($"Mensagem: {mensagem}");
            Console.WriteLine("========================================");

            await Task.CompletedTask;
            return true;
        }
    }
}