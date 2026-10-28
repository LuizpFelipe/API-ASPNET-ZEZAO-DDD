using Application.Request;
using Application.Services;
using Domain.Entities;
using Tests.Fakes;

namespace Tests;

public class TurmaServiceTest
{
    private readonly TurmaRepositorioEmMemoria _turmaRepositorio = new();
    private readonly CategoriaRepositorioEmMemoria _categoriaRepositorio = new();
    private readonly ProfessorRepositorioEmMemoria _professorRepositorio = new();
    private readonly Categoria _categoria = new("Sub-11", 11, 12);
    private readonly Professor _professor = new("Carlos", "44999990000", Guid.NewGuid());
    private readonly Professor _outroProfessor = new("Marcos", "44999990001", Guid.NewGuid());
    private readonly TurmaService _servico;

    public TurmaServiceTest()
    {
        _categoriaRepositorio.Categorias.Add(_categoria);
        _professorRepositorio.Professores.AddRange(new[] { _professor, _outroProfessor });
        _servico = new TurmaService(_turmaRepositorio, _categoriaRepositorio, _professorRepositorio);
    }

    private CriarTurmaRequestDTO CriarRequisicao(string nome, string dias, string horario, Professor professor) => new()
    {
        Nome = nome,
        DiasSemana = dias,
        Horario = horario,
        CategoriaId = _categoria.Id,
        ProfessorId = professor.Id
    };

    [Fact]
    public async Task Cria_Turma_Valida()
    {
        var resultado = await _servico.CriarAsync(CriarRequisicao("Turma A", "Segunda,Quarta", "17h", _professor));

        Assert.True(resultado.Status);
        Assert.Single(_turmaRepositorio.Turmas);
    }

    [Fact]
    public async Task Recusa_Turma_Com_Mesmo_Nome_Na_Mesma_Categoria()
    {
        await _servico.CriarAsync(CriarRequisicao("Turma A", "Segunda,Quarta", "17h", _professor));

        var resultado = await _servico.CriarAsync(CriarRequisicao("turma a", "Terça", "10h", _outroProfessor));

        Assert.False(resultado.Status);
        Assert.Single(_turmaRepositorio.Turmas);
    }

    [Fact]
    public async Task Recusa_Professor_Com_Duas_Turmas_No_Mesmo_Horario_E_Dia()
    {
        await _servico.CriarAsync(CriarRequisicao("Turma A", "Segunda,Quarta", "17h", _professor));

        var resultado = await _servico.CriarAsync(CriarRequisicao("Turma B", "Quarta,Sexta", "17h", _professor));

        Assert.False(resultado.Status);
        Assert.Contains("professor", resultado.Message);
    }

    [Fact]
    public async Task Aceita_Mesmo_Professor_Em_Horario_Ou_Dia_Diferente()
    {
        await _servico.CriarAsync(CriarRequisicao("Turma A", "Segunda,Quarta", "17h", _professor));

        var outroHorario = await _servico.CriarAsync(CriarRequisicao("Turma B", "Segunda", "18h", _professor));
        var outroDia = await _servico.CriarAsync(CriarRequisicao("Turma C", "Terça", "17h", _professor));

        Assert.True(outroHorario.Status);
        Assert.True(outroDia.Status);
    }

    [Fact]
    public async Task Aceita_Mesmo_Horario_Com_Professores_Diferentes()
    {
        await _servico.CriarAsync(CriarRequisicao("Turma A", "Segunda", "17h", _professor));

        Assert.True((await _servico.CriarAsync(CriarRequisicao("Turma B", "Segunda", "17h", _outroProfessor))).Status);
    }

    [Fact]
    public async Task Atualizar_Turma_Nao_Conflita_Com_Ela_Mesma()
    {
        await _servico.CriarAsync(CriarRequisicao("Turma A", "Segunda,Quarta", "17h", _professor));
        var turmaId = _turmaRepositorio.Turmas[0].Id;

        var resultado = await _servico.AtualizarAsync(turmaId, new AtualizarTurmaRequestDTO
        {
            Nome = "Turma A",
            DiasSemana = "Segunda,Quarta",
            Horario = "17h",
            CategoriaId = _categoria.Id,
            ProfessorId = _professor.Id
        });

        Assert.True(resultado.Status);
    }

    [Fact]
    public async Task Recusa_Horario_Invalido_Nome_Vazio_E_Dias_Invalidos()
    {
        Assert.False((await _servico.CriarAsync(CriarRequisicao("Turma", "Segunda", "25h", _professor))).Status);
        Assert.False((await _servico.CriarAsync(CriarRequisicao(" ", "Segunda", "17h", _professor))).Status);
        Assert.False((await _servico.CriarAsync(CriarRequisicao("Turma", "Domingão", "17h", _professor))).Status);
    }

    [Fact]
    public async Task Recusa_Categoria_Ou_Professor_Inexistente()
    {
        var semCategoria = CriarRequisicao("Turma", "Segunda", "17h", _professor);
        semCategoria.CategoriaId = Guid.NewGuid();
        var semProfessor = CriarRequisicao("Turma", "Segunda", "17h", _professor);
        semProfessor.ProfessorId = Guid.NewGuid();

        Assert.False((await _servico.CriarAsync(semCategoria)).Status);
        Assert.False((await _servico.CriarAsync(semProfessor)).Status);
    }
}
