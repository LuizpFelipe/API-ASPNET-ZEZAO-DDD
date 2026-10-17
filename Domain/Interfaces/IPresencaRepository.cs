using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPresencaRepository
    {
        // Acha o Professor a partir do nome de usuário que vem no token (Usuarios -> Professor).
        Task<Professor?> ObterProfessorPorNomeUsuarioAsync(string nomeUsuario);

        // professorId nulo = todas as turmas (usado para o Coordenador).
        Task<IEnumerable<Turma>> ObterTurmasAsync(Guid? professorId);

        Task<Turma?> ObterTurmaPorIdAsync(Guid turmaId);

        // Alunos da turma, sem os inativos.
        Task<IEnumerable<Aluno>> ObterAlunosDaTurmaAsync(Guid turmaId);

        // Presenças já registradas da turma na data (rastreadas, para poder alterar).
        Task<List<Presenca>> ObterPresencasAsync(Guid turmaId, DateOnly data);

        // Insere as novas e grava também as alterações feitas nas já existentes.
        Task SalvarChamadaAsync(IEnumerable<Presenca> novasPresencas);
    }
}