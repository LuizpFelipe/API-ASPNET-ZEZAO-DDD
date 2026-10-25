using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IWhatsAppService
    {
        Task<bool> EnviarMensagemAsync(string numeroTelefone, string mensagem);
    }
}