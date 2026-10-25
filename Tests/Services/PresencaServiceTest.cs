using Application.Configuration;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using NSubstitute;

namespace Tests;

public class PresencaServiceTest
{
    private static readonly DateOnly Hoje = new DateOnly(2026, 10, 30);

    private readonly IPresencaRepository _presencaRepository = Substitute.For<IPresencaRepository>();
    private readonly IRelogio _relogio = Substitute.For<IRelogio>();
    private readonly Professor _professor = new Professor("Carlos", "44999990000", Guid.NewGuid());
    private readonly Turma _turma;
    private readonly PresencaService _service;

    public PresencaServiceTest()
    {
        _relogio.HojeNoBrasil().Returns(Hoje);
        _turma = new Turma("Turma A", "Segunda,Terça,Sexta", "18h", Guid.NewGuid(), _professor.Id);

        _presencaRepository.ObterTurmaPorIdAsync(_turma.Id).Returns(_turma);
        _presencaRepository.ObterProfessorPorNomeUsuarioAsync("professor").Returns(_professor);
        _presencaRepository.ObterAlunosDaTurmaAsync(_turma.Id).Returns(new List<Aluno>());
        _presencaRepository.ObterPresencasAsync(_turma.Id, Arg.Any<DateOnly>()).Returns(new List<Presenca>());
        _presencaRepository.ObterPresencasDoMesAsync(_turma.Id, Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns(new List<Presenca>());
        _presencaRepository.ObterAlunosEmReposicaoAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>()).Returns(new List<Aluno>());

        _service = new PresencaService(_presencaRepository, _relogio, new ConfiguracoesPresenca { DiasLimiteEdicaoProfessor = 10 });
    }

    private Task<ChamadaResponseDTO> Registrar(DateOnly data, bool ehCoordenador)
    {
        var dto = new RegistrarPresencaRequestDTO { Data = data, Presencas = new List<PresencaAlunoRequestDTO>() };
        return _service.RegistrarChamadaAsync(_turma.Id, dto, ehCoordenador ? "coordenacao" : "professor", ehCoordenador);
    }

    [Fact]
    public async Task Professor_Nao_Pode_Alterar_Chamada_De_11_Dias_Atras()
    {
        var resultado = await Registrar(new DateOnly(2026, 10, 19), ehCoordenador: false);

        Assert.False(resultado.Status);
        Assert.Contains("últimos 10 dias", resultado.Message);
    }

    [Fact]
    public async Task Professor_Passa_Pela_Regra_De_Data_Na_Chamada_De_10_Dias_Atras()
    {
        var resultado = await Registrar(new DateOnly(2026, 10, 20), ehCoordenador: false);

        Assert.DoesNotContain("últimos 10 dias", resultado.Message);
        Assert.DoesNotContain("futura", resultado.Message);
        Assert.DoesNotContain("não tem aula", resultado.Message);
    }

    [Fact]
    public async Task Coordenador_Passa_Pela_Regra_De_Data_Em_Qualquer_Dia_De_Aula_Passado()
    {
        var resultado = await Registrar(new DateOnly(2026, 9, 7), ehCoordenador: true);

        Assert.DoesNotContain("últimos 10 dias", resultado.Message);
        Assert.DoesNotContain("futura", resultado.Message);
    }

    [Fact]
    public async Task Data_Futura_E_Recusada_Para_Todos()
    {
        var dataFutura = new DateOnly(2026, 11, 2);

        var doProfessor = await Registrar(dataFutura, ehCoordenador: false);
        var doCoordenador = await Registrar(dataFutura, ehCoordenador: true);

        Assert.Equal("Não é possível lançar chamada em data futura.", doProfessor.Message);
        Assert.Equal("Não é possível lançar chamada em data futura.", doCoordenador.Message);
    }

    [Fact]
    public async Task Dia_Sem_Aula_E_Recusado_Para_Todos()
    {
        var quarta = new DateOnly(2026, 10, 28);

        var doProfessor = await Registrar(quarta, ehCoordenador: false);
        var doCoordenador = await Registrar(quarta, ehCoordenador: true);

        Assert.Equal("A turma não tem aula nesta data.", doProfessor.Message);
        Assert.Equal("A turma não tem aula nesta data.", doCoordenador.Message);
    }

    [Fact]
    public async Task Consulta_Informa_Pode_Editar_De_Acordo_Com_O_Perfil_E_A_Data()
    {
        var dataAntiga = new DateOnly(2026, 10, 19);

        var doProfessor = await _service.ObterChamadaAsync(_turma.Id, dataAntiga, "professor", false);
        var doCoordenador = await _service.ObterChamadaAsync(_turma.Id, dataAntiga, "coordenacao", true);

        Assert.True(doProfessor.Status);
        Assert.False(doProfessor.Data!.PodeEditar);
        Assert.NotNull(doProfessor.Data.MotivoSomenteLeitura);
        Assert.True(doCoordenador.Data!.PodeEditar);
    }

    [Fact]
    public async Task Chamada_Mensal_Lista_Somente_Os_Dias_De_Aula_Do_Mes()
    {
        var resultado = await _service.ObterChamadaMensalAsync(_turma.Id, "2026-10", "professor", false);

        Assert.True(resultado.Status);
        Assert.Equal(13, resultado.Data!.Datas.Count);
        Assert.All(resultado.Data.Datas, item => Assert.True(_turma.TemAulaEm(item.Data)));
    }

    [Fact]
    public async Task Chamada_Mensal_Marca_Pode_Editar_Por_Data_Para_O_Professor()
    {
        var resultado = await _service.ObterChamadaMensalAsync(_turma.Id, "2026-10", "professor", false);

        var datas = resultado.Data!.Datas.ToDictionary(item => item.Data, item => item.PodeEditar);

        Assert.False(datas[new DateOnly(2026, 10, 19)]);
        Assert.True(datas[new DateOnly(2026, 10, 20)]);
        Assert.True(datas[new DateOnly(2026, 10, 30)]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("10/2026")]
    [InlineData("2026-13")]
    public async Task Chamada_Mensal_Recusa_Mes_Em_Formato_Invalido(string? mes)
    {
        var resultado = await _service.ObterChamadaMensalAsync(_turma.Id, mes, "professor", false);

        Assert.False(resultado.Status);
        Assert.Equal(TipoFalhaPresenca.Invalido, resultado.Falha);
    }

    [Fact]
    public async Task Professor_Nao_Consulta_O_Mes_De_Turma_De_Outro_Professor()
    {
        var turmaDeOutro = new Turma("Turma B", "Terça", "19h", Guid.NewGuid(), Guid.NewGuid());
        _presencaRepository.ObterTurmaPorIdAsync(turmaDeOutro.Id).Returns(turmaDeOutro);

        var resultado = await _service.ObterChamadaMensalAsync(turmaDeOutro.Id, "2026-10", "professor", false);

        Assert.False(resultado.Status);
        Assert.Equal(TipoFalhaPresenca.AcessoNegado, resultado.Falha);
    }

    [Fact]
    public async Task Listagem_Por_Dia_Mostra_So_As_Turmas_Com_Aula_No_Dia()
    {
        var turmaDeQuarta = new Turma("Turma Q", "Quarta", "18h", Guid.NewGuid(), _professor.Id);
        _presencaRepository.ObterTurmasAsync(_professor.Id).Returns(new List<Turma> { _turma, turmaDeQuarta });

        var segunda = await _service.ListarTurmasDoProfessorAsync("professor", false, new DateOnly(2026, 10, 26));
        var quarta = await _service.ListarTurmasDoProfessorAsync("professor", false, new DateOnly(2026, 10, 28));
        var sabado = await _service.ListarTurmasDoProfessorAsync("professor", false, new DateOnly(2026, 10, 31));

        Assert.Equal(new[] { _turma.Id }, segunda.Data.Select(item => item.Id));
        Assert.Equal(new[] { turmaDeQuarta.Id }, quarta.Data.Select(item => item.Id));
        Assert.Empty(sabado.Data);
        Assert.Equal("Nenhuma aula neste dia.", sabado.Message);
    }

    [Fact]
    public async Task Listagem_Sem_Data_Devolve_Todas_As_Turmas()
    {
        _presencaRepository.ObterTurmasAsync(_professor.Id).Returns(new List<Turma> { _turma });

        var resultado = await _service.ListarTurmasDoProfessorAsync("professor", false);

        Assert.Single(resultado.Data);
        Assert.Null(resultado.Data[0].TemAulaNaData);
    }

    [Fact]
    public async Task Coordenador_Filtra_Por_Professor_E_Professor_Ignora_O_Filtro()
    {
        var outroProfessorId = Guid.NewGuid();
        _presencaRepository.ObterTurmasAsync(outroProfessorId).Returns(new List<Turma>());
        _presencaRepository.ObterTurmasAsync(_professor.Id).Returns(new List<Turma> { _turma });

        var doCoordenador = await _service.ListarTurmasDoProfessorAsync("coordenacao", true, null, outroProfessorId);
        var doProfessor = await _service.ListarTurmasDoProfessorAsync("professor", false, null, outroProfessorId);

        Assert.Empty(doCoordenador.Data);
        Assert.Single(doProfessor.Data);
    }
}