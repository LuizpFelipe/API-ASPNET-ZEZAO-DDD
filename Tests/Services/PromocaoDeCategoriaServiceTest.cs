using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Tests.Fakes;

namespace Tests.Services;

public class PromocaoDeCategoriaServiceTest
{
    private static readonly DateOnly Hoje = new(2026, 10, 30);

    private readonly AlunoRepositorioEmMemoria _alunos = new();
    private readonly CategoriaRepositorioEmMemoria _categorias = new();
    private readonly TurmaRepositorioEmMemoria _turmas = new();
    private readonly WhatsAppDeTeste _whatsApp = new();
    private readonly PromocaoDeCategoriaService _servico;

    private readonly Categoria _sub11 = new("Sub-11", 9, 11);
    private readonly Categoria _sub13 = new("Sub-13", 12, 13);
    private readonly Categoria _sub15 = new("Sub-15", 14, 15);
    private readonly Guid _professorId = Guid.NewGuid();

    public PromocaoDeCategoriaServiceTest()
    {
        _categorias.Categorias.AddRange(new[] { _sub11, _sub13, _sub15 });
        _servico = new PromocaoDeCategoriaService(_alunos, _categorias, _turmas, _whatsApp, new RelogioFixo(Hoje));
    }

    private Turma CriarTurma(Categoria categoria, string nome, string horario = "17h", Guid? professorId = null)
    {
        var turma = new Turma(nome, "Seg,Qua", horario, categoria.Id, professorId ?? _professorId);
        _turmas.Turmas.Add(turma);
        return turma;
    }

    private Aluno CriarAluno(Categoria categoria, Turma turma, DateOnly nascimento, string nome = "Joao Silva")
    {
        var aluno = new Aluno(nome, nascimento, NivelTecnico.Iniciante, null, "Maria", "52998224725",
            "(41) 98888-7777", "Rua A", turma.Id, categoria.Id);
        _alunos.Alunos.Add(aluno);
        return aluno;
    }

    // Em 30/10/2026: nascido em 30/10/2014 faz 12 hoje; nascido em 31/10/2014 ainda tem 11.

    [Fact]
    public async Task Aluno_Que_Faz_Aniversario_E_Entra_Na_Faixa_Seguinte_Sobe_De_Categoria_E_Turma()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A");
        var turmaNova = CriarTurma(_sub13, "Sub-13 A");
        var aluno = CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 10, 30));

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AlunosPromovidos);
        Assert.Equal(_sub13.Id, aluno.CategoriaId);
        Assert.Equal(turmaNova.Id, aluno.TurmaId);
    }

    [Fact]
    public async Task Aluno_Que_Ainda_Nao_Fez_Aniversario_Fica_Onde_Esta()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A");
        CriarTurma(_sub13, "Sub-13 A");
        var aluno = CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 10, 31));

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AlunosPromovidos);
        Assert.Equal(_sub11.Id, aluno.CategoriaId);
        Assert.Equal(turmaAntiga.Id, aluno.TurmaId);
    }

    [Fact]
    public async Task Aluno_Nunca_Desce_De_Categoria()
    {
        var turmaAtual = CriarTurma(_sub13, "Sub-13 A");
        CriarTurma(_sub11, "Sub-11 A");
        // 10 anos de idade, mas está matriculado na Sub-13: não pode regredir.
        var aluno = CriarAluno(_sub13, turmaAtual, new DateOnly(2016, 1, 1));

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AlunosPromovidos);
        Assert.Equal(_sub13.Id, aluno.CategoriaId);
    }

    [Fact]
    public async Task Aluno_Na_Categoria_Certa_Nao_E_Alterado()
    {
        var turma = CriarTurma(_sub13, "Sub-13 A");
        var aluno = CriarAluno(_sub13, turma, new DateOnly(2013, 5, 5));

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AlunosPromovidos);
        Assert.Equal(turma.Id, aluno.TurmaId);
    }

    [Fact]
    public async Task Sem_Turma_Na_Nova_Categoria_Nao_Promove_E_Registra_Pendencia()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A");
        var aluno = CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 1, 1), "Pedro Lima");

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AlunosPromovidos);
        Assert.Equal(_sub11.Id, aluno.CategoriaId);
        var pendencia = Assert.Single(resultado.AlunosSemTurmaNaNovaCategoria);
        Assert.Contains("Pedro Lima", pendencia);
        Assert.Contains("Sub-13", pendencia);
    }

    [Fact]
    public async Task Prefere_Turma_Do_Mesmo_Professor_E_Horario()
    {
        var outroProfessor = Guid.NewGuid();
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A", "17h", _professorId);
        CriarTurma(_sub13, "Sub-13 A", "18h", outroProfessor);
        CriarTurma(_sub13, "Sub-13 B", "18h", _professorId);
        var ideal = CriarTurma(_sub13, "Sub-13 C", "17h", _professorId);
        var aluno = CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 1, 1));

        await _servico.ExecutarAsync();

        Assert.Equal(ideal.Id, aluno.TurmaId);
    }

    [Fact]
    public async Task Sem_Mesmo_Professor_Usa_Turma_Do_Mesmo_Horario()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A", "17h", _professorId);
        CriarTurma(_sub13, "Sub-13 A", "18h", Guid.NewGuid());
        var mesmoHorario = CriarTurma(_sub13, "Sub-13 B", "17h", Guid.NewGuid());
        var aluno = CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 1, 1));

        await _servico.ExecutarAsync();

        Assert.Equal(mesmoHorario.Id, aluno.TurmaId);
    }

    [Fact]
    public async Task Aluno_Inativo_Nao_E_Promovido()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A");
        CriarTurma(_sub13, "Sub-13 A");
        var aluno = CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 1, 1));
        aluno.Desativar();
        _alunos.Alunos.Add(aluno);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AlunosPromovidos);
        Assert.Equal(_sub11.Id, aluno.CategoriaId);
    }

    [Fact]
    public async Task Idade_Acima_De_Todas_As_Categorias_Nao_Muda_Nada()
    {
        var turma = CriarTurma(_sub15, "Sub-15 A");
        var aluno = CriarAluno(_sub15, turma, new DateOnly(2008, 1, 1));

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AlunosPromovidos);
        Assert.Equal(_sub15.Id, aluno.CategoriaId);
    }

    [Fact]
    public async Task Rodar_De_Novo_Nao_Promove_Duas_Vezes_Nem_Reavisa()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A");
        CriarTurma(_sub13, "Sub-13 A");
        CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 1, 1));

        await _servico.ExecutarAsync();
        var segunda = await _servico.ExecutarAsync();

        Assert.Equal(0, segunda.AlunosPromovidos);
        Assert.Equal(1, _whatsApp.Mensagens.Count);
    }

    [Fact]
    public async Task Responsavel_E_Avisado_Da_Nova_Turma()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A");
        CriarTurma(_sub13, "Sub-13 A", "18h");
        CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 1, 1));

        await _servico.ExecutarAsync();

        var mensagem = Assert.Single(_whatsApp.Mensagens);
        Assert.Equal("5541988887777", mensagem.Numero);
        Assert.Contains("Sub-13", mensagem.Texto);
        Assert.Contains("18h", mensagem.Texto);
    }

    [Fact]
    public async Task Falha_No_Aviso_Nao_Desfaz_A_Promocao()
    {
        var turmaAntiga = CriarTurma(_sub11, "Sub-11 A");
        CriarTurma(_sub13, "Sub-13 A");
        var aluno = CriarAluno(_sub11, turmaAntiga, new DateOnly(2014, 1, 1));
        _whatsApp.Falhar = true;

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AlunosPromovidos);
        Assert.Equal(_sub13.Id, aluno.CategoriaId);
    }

    private sealed class RelogioFixo : IRelogio
    {
        private readonly DateOnly _hoje;
        public RelogioFixo(DateOnly hoje) { _hoje = hoje; }
        public DateOnly HojeNoBrasil() => _hoje;
        public DateTime AgoraNoBrasil() => _hoje.ToDateTime(new TimeOnly(12, 0));
    }

    private sealed class WhatsAppDeTeste : IWhatsAppService
    {
        public List<(string Numero, string Texto)> Mensagens { get; } = new();
        public bool Falhar { get; set; }

        public Task<bool> EnviarMensagemAsync(string numeroTelefone, string mensagem)
        {
            if (Falhar) throw new InvalidOperationException("WhatsApp indisponível");
            Mensagens.Add((numeroTelefone, mensagem));
            return Task.FromResult(true);
        }
    }
}
