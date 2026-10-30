using System.Threading.Tasks;
using Application.Response;

namespace Application.Interfaces
{
    public interface IRotinaFinanceiraService
    {
        /// <summary>Marca como Inadimplente quem tem parcela vencida e volta a Ativo quem regularizou.</summary>
        Task<(int Marcados, int Regularizados)> SincronizarStatusDosAlunosAsync();

        /// <summary>Sincroniza o status dos alunos e dispara os avisos automáticos de cobrança.</summary>
        Task<ResultadoDaRotinaFinanceiraDTO> ExecutarAsync();
    }
}
