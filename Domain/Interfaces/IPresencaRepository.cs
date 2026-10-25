using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPresencaRepository
    {
        Task<Professor?> ObterProfessorPorNomeUsuarioAsync(string nomeUsuario);

        Task<IEnumerable<Turma>> ObterTurmasAsync(Guid? professorId);

        Task<Turma?> ObterTurmaPorIdAsync(Guid turmaId);
        Task<IEnumerable<Aluno>> ObterAlunosDaTurmaAsync(Guid turmaId);

        Task<List<Presenca>> ObterPresencasAsync(Guid turmaId, DateOnly data);

        Task SalvarChamadaAsync(IEnumerable<Presenca> novasPresencas);
        Task<List<Presenca>> ObterPresencasDoMesAsync(Guid turmaId, DateOnly inicio, DateOnly fim);
        Task<List<Aluno>> ObterAlunosEmReposicaoAsync(Guid turmaDestinoId, DateOnly data);
    }
}