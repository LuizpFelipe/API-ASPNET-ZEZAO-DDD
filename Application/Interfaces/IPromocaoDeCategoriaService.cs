using System.Threading.Tasks;
using Application.Response;

namespace Application.Interfaces
{
    public interface IPromocaoDeCategoriaService
    {
        /// <summary>
        /// Sobe de categoria (e de turma) os alunos cuja idade já pertence a uma categoria acima da atual.
        /// O aluno nunca desce de categoria.
        /// </summary>
        Task<ResultadoDaPromocaoDeCategoriaDTO> ExecutarAsync();
    }
}
