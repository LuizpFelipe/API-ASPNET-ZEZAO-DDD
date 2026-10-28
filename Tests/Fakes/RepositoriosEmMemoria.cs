using Domain.Entities;
using Domain.Interfaces;

namespace Tests.Fakes;

public class CampeonatoRepositorioEmMemoria : ICampeonatoRepository
{
    public List<Campeonato> Campeonatos { get; } = new();
    public List<ParticipacaoCampeonato> Participacoes { get; } = new();
    public Func<bool> PossuiJogos { get; set; } = () => false;

    public Task<IEnumerable<Campeonato>> ObterTodosAsync() => Task.FromResult<IEnumerable<Campeonato>>(Campeonatos.ToList());
    public Task<Campeonato?> ObterPorIdAsync(Guid id) => Task.FromResult(Campeonatos.FirstOrDefault(campeonato => campeonato.Id == id));
    public Task AdicionarAsync(Campeonato campeonato) { Campeonatos.Add(campeonato); return Task.CompletedTask; }
    public Task AtualizarAsync(Campeonato campeonato) => Task.CompletedTask;
    public Task RemoverAsync(Campeonato campeonato) { Campeonatos.Remove(campeonato); return Task.CompletedTask; }
    public Task AdicionarParticipacaoAsync(ParticipacaoCampeonato participacao) { Participacoes.Add(participacao); return Task.CompletedTask; }
    public Task<ParticipacaoCampeonato?> ObterParticipacaoPorIdAsync(Guid id) => Task.FromResult(Participacoes.FirstOrDefault(participacao => participacao.Id == id));
    public Task AtualizarParticipacaoAsync(ParticipacaoCampeonato participacao) => Task.CompletedTask;
    public Task<IEnumerable<ParticipacaoCampeonato>> ObterParticipacoesPorCampeonatoAsync(Guid campeonatoId) =>
        Task.FromResult<IEnumerable<ParticipacaoCampeonato>>(Participacoes.Where(participacao => participacao.CampeonatoId == campeonatoId).ToList());
    public Task<bool> PossuiJogosAsync(Guid campeonatoId) => Task.FromResult(PossuiJogos());
}

public class AlunoRepositorioEmMemoria : IAlunoRepository
{
    public List<Aluno> Alunos { get; } = new();

    public Task<IEnumerable<Aluno>> ObterTodosAsync() =>
        Task.FromResult<IEnumerable<Aluno>>(Alunos.Where(aluno => aluno.Status != Domain.Enums.StatusAluno.Inativo).ToList());
    public Task<Aluno?> ObterPorIdAsync(Guid id) => Task.FromResult(Alunos.FirstOrDefault(aluno => aluno.Id == id));
    public Task<Dictionary<Guid, string>> ObterNomesPorIdsAsync(IEnumerable<Guid> alunoIds) =>
        Task.FromResult(Alunos.Where(aluno => alunoIds.Contains(aluno.Id)).ToDictionary(aluno => aluno.Id, aluno => aluno.NomeCompleto));
    public Task<List<Aluno>> ObterPorIdsAsync(IEnumerable<Guid> alunoIds)
    {
        var identificadores = alunoIds.ToHashSet();
        return Task.FromResult(Alunos.Where(aluno => identificadores.Contains(aluno.Id)).ToList());
    }
    public Task<bool> ExisteComMesmoNomeEDataNascimentoAsync(string nome, DateOnly dataNascimento, Guid? ignorarId = null) =>
        Task.FromResult(Alunos.Any(aluno => aluno.NomeCompleto == nome && aluno.DataNascimento == dataNascimento && aluno.Id != ignorarId));
    public Task AdicionarAsync(Aluno aluno) { Alunos.Add(aluno); return Task.CompletedTask; }
    public Task AtualizarAsync(Aluno aluno) => Task.CompletedTask;
}

public class CategoriaRepositorioEmMemoria : ICategoriaRepository
{
    public List<Categoria> Categorias { get; } = new();

    public Task<IEnumerable<Categoria>> ObterTodasAsync() => Task.FromResult<IEnumerable<Categoria>>(Categorias.ToList());
    public Task<Categoria?> ObterPorIdAsync(Guid id) => Task.FromResult(Categorias.FirstOrDefault(categoria => categoria.Id == id));
    public Task<Categoria?> ObterPorIdadeAsync(int idade) =>
        Task.FromResult(Categorias.FirstOrDefault(categoria => idade >= categoria.IdadeMin && idade <= categoria.IdadeMax));
    public Task<bool> ExisteSobreposicaoDeIdadeAsync(int idadeMin, int idadeMax, Guid? ignorarId = null) =>
        Task.FromResult(Categorias.Any(categoria => categoria.Id != ignorarId && idadeMin <= categoria.IdadeMax && idadeMax >= categoria.IdadeMin));
    public Task AdicionarAsync(Categoria categoria) { Categorias.Add(categoria); return Task.CompletedTask; }
    public Task AtualizarAsync(Categoria categoria) => Task.CompletedTask;
    public Task RemoverAsync(Categoria categoria) { Categorias.Remove(categoria); return Task.CompletedTask; }
    public Task<bool> PossuiVinculosAsync(Guid categoriaId) => Task.FromResult(false);
}

public class TurmaRepositorioEmMemoria : ITurmaRepository
{
    public List<Turma> Turmas { get; } = new();

    public Task<IEnumerable<Turma>> ObterTodasAsync() => Task.FromResult<IEnumerable<Turma>>(Turmas.ToList());
    public Task<Turma> ObterPorIdAsync(Guid id) => Task.FromResult(Turmas.FirstOrDefault(turma => turma.Id == id)!);
    public Task<IEnumerable<Turma>> ObterPorCategoriaAsync(Guid categoriaId) =>
        Task.FromResult<IEnumerable<Turma>>(Turmas.Where(turma => turma.CategoriaId == categoriaId).ToList());
    public Task<int> ContarAlunosPorTurmaAsync(Guid turmaId) => Task.FromResult(0);
    public Task AdicionarAsync(Turma turma) { Turmas.Add(turma); return Task.CompletedTask; }
    public Task AtualizarAsync(Turma turma) => Task.CompletedTask;
    public Task RemoverAsync(Turma turma) { Turmas.Remove(turma); return Task.CompletedTask; }
    public Task<bool> PossuiVinculosAsync(Guid turmaId) => Task.FromResult(false);
}

public class ProfessorRepositorioEmMemoria : IProfessorRepository
{
    public List<Professor> Professores { get; } = new();

    public Task<IEnumerable<Professor>> ObterTodosAsync() => Task.FromResult<IEnumerable<Professor>>(Professores.ToList());
    public Task<Professor?> ObterPorIdAsync(Guid id) => Task.FromResult(Professores.FirstOrDefault(professor => professor.Id == id));
    public Task AdicionarAsync(Professor professor) { Professores.Add(professor); return Task.CompletedTask; }
    public Task AtualizarAsync(Professor professor) => Task.CompletedTask;
    public Task RemoverAsync(Professor professor) { Professores.Remove(professor); return Task.CompletedTask; }
    public Task<bool> PossuiTurmasAsync(Guid professorId) => Task.FromResult(false);
}

public class CompeticaoRepositorioEmMemoria : ICampeonatoCompeticaoRepository
{
    public List<EquipeCampeonato> Equipes { get; } = new();
    public List<ParticipacaoCampeonato> Participacoes { get; } = new();
    public List<JogoCampeonato> Jogos { get; } = new();
    public List<EstatisticaJogo> Estatisticas { get; } = new();

    public Task<List<EquipeCampeonato>> ObterEquipesAsync(Guid campeonatoId) => Task.FromResult(Equipes.Where(equipe => equipe.CampeonatoId == campeonatoId).ToList());
    public Task<EquipeCampeonato?> ObterEquipePorIdAsync(Guid equipeId) => Task.FromResult(Equipes.FirstOrDefault(equipe => equipe.Id == equipeId));
    public Task AdicionarEquipesAsync(IEnumerable<EquipeCampeonato> equipes, IEnumerable<ParticipacaoCampeonato> participacoesAtualizadas) { Equipes.AddRange(equipes); return Task.CompletedTask; }
    public Task RemoverEquipeAsync(EquipeCampeonato equipe, IEnumerable<ParticipacaoCampeonato> participacoesLiberadas) { Equipes.Remove(equipe); return Task.CompletedTask; }
    public Task<List<ParticipacaoCampeonato>> ObterParticipacoesAsync(Guid campeonatoId) => Task.FromResult(Participacoes.Where(participacao => participacao.CampeonatoId == campeonatoId).ToList());
    public Task<ParticipacaoCampeonato?> ObterParticipacaoPorIdAsync(Guid participacaoId) => Task.FromResult(Participacoes.FirstOrDefault(participacao => participacao.Id == participacaoId));
    public Task RemoverParticipacaoAsync(ParticipacaoCampeonato participacao) { Participacoes.Remove(participacao); return Task.CompletedTask; }
    public Task<List<JogoCampeonato>> ObterJogosAsync(Guid campeonatoId) => Task.FromResult(Jogos.Where(jogo => jogo.CampeonatoId == campeonatoId).ToList());
    public Task<JogoCampeonato?> ObterJogoPorIdAsync(Guid jogoId) => Task.FromResult(Jogos.FirstOrDefault(jogo => jogo.Id == jogoId));
    public Task AdicionarJogosAsync(IEnumerable<JogoCampeonato> jogos) { Jogos.AddRange(jogos); return Task.CompletedTask; }
    public Task AtualizarJogosAsync(IEnumerable<JogoCampeonato> jogos) => Task.CompletedTask;
    public Task RemoverJogosDoCampeonatoAsync(Guid campeonatoId) { Jogos.RemoveAll(jogo => jogo.CampeonatoId == campeonatoId); return Task.CompletedTask; }
    public Task<List<EstatisticaJogo>> ObterEstatisticasDoJogoAsync(Guid jogoId) => Task.FromResult(Estatisticas.Where(estatistica => estatistica.JogoId == jogoId).ToList());
    public Task<List<EstatisticaJogo>> ObterEstatisticasDoCampeonatoAsync(Guid campeonatoId)
    {
        var jogosDoCampeonato = Jogos.Where(jogo => jogo.CampeonatoId == campeonatoId).Select(jogo => jogo.Id).ToHashSet();
        return Task.FromResult(Estatisticas.Where(estatistica => jogosDoCampeonato.Contains(estatistica.JogoId)).ToList());
    }
    public Task SubstituirEstatisticasDoJogoAsync(Guid jogoId, IEnumerable<EstatisticaJogo> estatisticas)
    {
        Estatisticas.RemoveAll(estatistica => estatistica.JogoId == jogoId);
        Estatisticas.AddRange(estatisticas);
        return Task.CompletedTask;
    }
}
