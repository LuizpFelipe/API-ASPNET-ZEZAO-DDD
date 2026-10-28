using Application.Request;
using Application.Response;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Tests.Fakes;

namespace Tests;

public class CampeonatoServiceTest
{
    private readonly CampeonatoRepositorioEmMemoria _campeonatoRepositorio = new();
    private readonly AlunoRepositorioEmMemoria _alunoRepositorio = new();
    private readonly CategoriaRepositorioEmMemoria _categoriaRepositorio = new();
    private readonly Categoria _sub9 = new("Sub-9", 7, 8);
    private readonly Categoria _sub10 = new("Sub-10", 9, 10);
    private readonly Categoria _sub11 = new("Sub-11", 11, 12);
    private readonly Categoria _sub13 = new("Sub-13", 13, 14);
    private readonly Campeonato _campeonato;
    private readonly CampeonatoService _servico;

    public CampeonatoServiceTest()
    {
        _categoriaRepositorio.Categorias.AddRange(new[] { _sub9, _sub10, _sub11, _sub13 });
        _campeonato = new Campeonato("Copa", new DateOnly(2026, 11, 1), _sub11.Id);
        _campeonatoRepositorio.Campeonatos.Add(_campeonato);
        _servico = new CampeonatoService(_campeonatoRepositorio, _categoriaRepositorio, _alunoRepositorio);
    }

    private Aluno CriarAluno(Categoria categoria, PosicaoJogador? posicao = PosicaoJogador.Fixo, string nome = "Aluno")
    {
        var aluno = new Aluno(nome + Guid.NewGuid(), new DateOnly(2015, 1, 1), NivelTecnico.Iniciante, null, "Resp", "12345678909", "1", "Rua", Guid.NewGuid(), categoria.Id, posicao);
        _alunoRepositorio.Alunos.Add(aluno);
        return aluno;
    }

    private Task<ParticipacaoCampeonatoResponseDTO> Inscrever(Aluno aluno) =>
        _servico.InscreverAlunoAsync(_campeonato.Id, new InscreverAlunoCampeonatoRequestDTO { AlunoId = aluno.Id });

    [Fact]
    public async Task Inscreve_Aluno_Da_Mesma_Categoria()
    {
        var resultado = await Inscrever(CriarAluno(_sub11));

        Assert.True(resultado.Status);
        Assert.Single(_campeonatoRepositorio.Participacoes);
    }

    [Fact]
    public async Task Inscreve_Aluno_Da_Categoria_Imediatamente_Abaixo()
    {
        Assert.True((await Inscrever(CriarAluno(_sub10))).Status);
    }

    [Fact]
    public async Task Recusa_Aluno_De_Duas_Categorias_Abaixo()
    {
        Assert.False((await Inscrever(CriarAluno(_sub9))).Status);
    }

    [Fact]
    public async Task Recusa_Aluno_De_Categoria_Acima()
    {
        Assert.False((await Inscrever(CriarAluno(_sub13))).Status);
    }

    [Fact]
    public async Task Recusa_Inscricao_Duplicada()
    {
        var aluno = CriarAluno(_sub11);
        await Inscrever(aluno);

        var resultado = await Inscrever(aluno);

        Assert.False(resultado.Status);
        Assert.Contains("já está inscrito", resultado.Message);
        Assert.Single(_campeonatoRepositorio.Participacoes);
    }

    [Fact]
    public async Task Recusa_Aluno_Inativo()
    {
        var aluno = CriarAluno(_sub11);
        aluno.Desativar();

        var resultado = await Inscrever(aluno);

        Assert.False(resultado.Status);
        Assert.Contains("inativo", resultado.Message);
    }

    [Fact]
    public async Task Recusa_Aluno_Sem_Posicao()
    {
        var resultado = await Inscrever(CriarAluno(_sub11, posicao: null));

        Assert.False(resultado.Status);
        Assert.Contains("posição", resultado.Message);
    }

    [Fact]
    public async Task Recusa_Inscricao_Depois_Dos_Jogos_Gerados()
    {
        _campeonatoRepositorio.PossuiJogos = () => true;

        var resultado = await Inscrever(CriarAluno(_sub11));

        Assert.False(resultado.Status);
    }

    [Fact]
    public async Task Recusa_Aluno_Ou_Campeonato_Inexistente()
    {
        var semAluno = await _servico.InscreverAlunoAsync(_campeonato.Id, new InscreverAlunoCampeonatoRequestDTO { AlunoId = Guid.NewGuid() });
        var semCampeonato = await _servico.InscreverAlunoAsync(Guid.NewGuid(), new InscreverAlunoCampeonatoRequestDTO { AlunoId = Guid.NewGuid() });

        Assert.False(semAluno.Status);
        Assert.False(semCampeonato.Status);
    }

    [Fact]
    public async Task Lista_Participacoes_Com_Nome_E_Posicao_Do_Aluno()
    {
        var aluno = CriarAluno(_sub11, PosicaoJogador.Goleiro);
        await Inscrever(aluno);

        var resultado = await _servico.ListarParticipacoesAsync(_campeonato.Id);

        var participacoes = Assert.IsType<List<ParticipacaoCampeonatoDataDTO>>(resultado.Data);
        Assert.Equal(aluno.NomeCompleto, participacoes[0].AlunoNome);
        Assert.Equal("Goleiro", participacoes[0].PosicaoDeJogo);
    }

    [Fact]
    public async Task Atualizar_Participacao_Recusa_Aproveitamento_Acima_De_100()
    {
        var aluno = CriarAluno(_sub11);
        await Inscrever(aluno);
        var participacaoId = _campeonatoRepositorio.Participacoes[0].Id;

        var resultado = await _servico.AtualizarParticipacaoAsync(participacaoId,
            new AtualizarParticipacaoCampeonatoRequestDTO { JogosDisputados = 1, Aproveitamento = 150 });

        Assert.False(resultado.Status);
    }

    [Fact]
    public async Task Atualizar_Participacao_Recusa_Jogos_Negativos()
    {
        await Inscrever(CriarAluno(_sub11));
        var participacaoId = _campeonatoRepositorio.Participacoes[0].Id;

        var resultado = await _servico.AtualizarParticipacaoAsync(participacaoId,
            new AtualizarParticipacaoCampeonatoRequestDTO { JogosDisputados = -1, Aproveitamento = 10 });

        Assert.False(resultado.Status);
    }

    [Fact]
    public async Task Criar_Campeonato_Recusa_Nome_Vazio_E_Categoria_Inexistente()
    {
        var semNome = await _servico.CriarAsync(new CriarCampeonatoRequestDTO { Nome = " ", Data = new DateOnly(2026, 12, 1), CategoriaId = _sub11.Id });
        var semCategoria = await _servico.CriarAsync(new CriarCampeonatoRequestDTO { Nome = "Copa", Data = new DateOnly(2026, 12, 1), CategoriaId = Guid.NewGuid() });

        Assert.False(semNome.Status);
        Assert.False(semCategoria.Status);
    }
}
