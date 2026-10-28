using Application.Request;
using Application.Response;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Tests.Fakes;

namespace Tests;

public class CampeonatoCompeticaoServiceTest
{
    private readonly CampeonatoRepositorioEmMemoria _campeonatoRepositorio = new();
    private readonly CompeticaoRepositorioEmMemoria _competicaoRepositorio = new();
    private readonly AlunoRepositorioEmMemoria _alunoRepositorio = new();
    private readonly CategoriaRepositorioEmMemoria _categoriaRepositorio = new();
    private readonly Categoria _sub10 = new("Sub-10", 9, 10);
    private readonly Categoria _sub11 = new("Sub-11", 11, 12);
    private readonly Campeonato _campeonato;
    private readonly CampeonatoCompeticaoService _servico;
    private int _contadorDeAlunos;

    public CampeonatoCompeticaoServiceTest()
    {
        _categoriaRepositorio.Categorias.AddRange(new[] { _sub10, _sub11 });
        _campeonato = new Campeonato("Copa", new DateOnly(2026, 11, 1), _sub11.Id);
        _campeonatoRepositorio.Campeonatos.Add(_campeonato);
        _campeonatoRepositorio.PossuiJogos = () => _competicaoRepositorio.Jogos.Count > 0;
        _servico = new CampeonatoCompeticaoService(_campeonatoRepositorio, _competicaoRepositorio, _alunoRepositorio, _categoriaRepositorio);
    }

    private Aluno CriarEInscrever(Categoria categoria, PosicaoJogador? posicao, bool inscrever = true)
    {
        _contadorDeAlunos++;
        var aluno = new Aluno($"Aluno {_contadorDeAlunos}", new DateOnly(2015, 1, 1), NivelTecnico.Iniciante, null, "Resp", "12345678909", "1", "Rua", Guid.NewGuid(), categoria.Id, posicao);
        _alunoRepositorio.Alunos.Add(aluno);
        if (inscrever)
            _competicaoRepositorio.Participacoes.Add(new ParticipacaoCampeonato(_campeonato.Id, aluno.Id, null));
        return aluno;
    }

    private void InscreverAlunosDeCadaPosicao(int quantidadePorPosicao)
    {
        foreach (var posicao in Enum.GetValues<PosicaoJogador>())
            for (var indice = 0; indice < quantidadePorPosicao; indice++)
                CriarEInscrever(_sub11, posicao);
    }

    private async Task<List<JogoDTO>> GerarJogosComTimes(int quantidadeDeTimes)
    {
        InscreverAlunosDeCadaPosicao(quantidadeDeTimes);
        await _servico.SortearEquipesAsync(_campeonato.Id, new SortearEquipesRequestDTO { Quantidade = quantidadeDeTimes });
        return (await _servico.GerarJogosAsync(_campeonato.Id)).Data!;
    }

    private async Task Jogar(JogoDTO jogo, int golsDoTimeA, int golsDoTimeB)
    {
        var estatisticas = (await _servico.ObterEstatisticasAsync(jogo.Id)).Data!;
        var itens = estatisticas.Jogadores.Select(jogador => new EstatisticaDoJogadorRequestDTO { AlunoId = jogador.AlunoId }).ToList();
        var pivoDoTimeA = estatisticas.Jogadores.First(jogador => jogador.EquipeId == jogo.EquipeAId && jogador.Posicao == (int)PosicaoJogador.Pivo);
        var pivoDoTimeB = estatisticas.Jogadores.First(jogador => jogador.EquipeId == jogo.EquipeBId && jogador.Posicao == (int)PosicaoJogador.Pivo);
        itens.First(item => item.AlunoId == pivoDoTimeA.AlunoId).Gols = golsDoTimeA;
        itens.First(item => item.AlunoId == pivoDoTimeB.AlunoId).Gols = golsDoTimeB;
        var salvo = await _servico.SalvarEstatisticasAsync(jogo.Id, new SalvarEstatisticasRequestDTO { Jogadores = itens });
        Assert.True(salvo.Status);
    }

    [Fact]
    public async Task Lista_Como_Elegiveis_Apenas_Quem_Nao_Esta_Inscrito_Da_Categoria_E_Da_Abaixo()
    {
        var disponivel = CriarEInscrever(_sub10, PosicaoJogador.Fixo, inscrever: false);
        CriarEInscrever(_sub11, PosicaoJogador.Fixo, inscrever: true);
        var outraCategoria = new Categoria("Sub-13", 13, 14);
        _categoriaRepositorio.Categorias.Add(outraCategoria);
        CriarEInscrever(outraCategoria, PosicaoJogador.Fixo, inscrever: false);

        var resultado = await _servico.ListarElegiveisAsync(_campeonato.Id);

        Assert.Single(resultado.Data!);
        Assert.Equal(disponivel.Id, resultado.Data![0].AlunoId);
    }

    [Fact]
    public async Task Cria_Time_Manual_Com_Uma_Posicao_De_Cada()
    {
        InscreverAlunosDeCadaPosicao(1);
        var alunoIds = _alunoRepositorio.Alunos.Select(aluno => aluno.Id).ToList();

        var resultado = await _servico.CriarEquipeAsync(_campeonato.Id, new CriarEquipeRequestDTO { Nome = "Azuis", AlunoIds = alunoIds });

        Assert.True(resultado.Status);
        Assert.Equal("Azuis", resultado.Data!.Nome);
        Assert.Equal(5, resultado.Data.Jogadores.Count);
    }

    [Fact]
    public async Task Recusa_Time_Com_Posicao_Repetida()
    {
        InscreverAlunosDeCadaPosicao(1);
        var extra = CriarEInscrever(_sub11, PosicaoJogador.Goleiro);
        var alunoIds = _alunoRepositorio.Alunos.Where(aluno => aluno.Posicao != PosicaoJogador.Pivo).Select(aluno => aluno.Id).ToList();
        Assert.Equal(5, alunoIds.Count);
        Assert.Contains(extra.Id, alunoIds);

        var resultado = await _servico.CriarEquipeAsync(_campeonato.Id, new CriarEquipeRequestDTO { AlunoIds = alunoIds });

        Assert.False(resultado.Status);
    }

    [Fact]
    public async Task Recusa_Time_Com_Menos_De_Cinco_Alunos()
    {
        InscreverAlunosDeCadaPosicao(1);
        var alunoIds = _alunoRepositorio.Alunos.Take(4).Select(aluno => aluno.Id).ToList();

        Assert.False((await _servico.CriarEquipeAsync(_campeonato.Id, new CriarEquipeRequestDTO { AlunoIds = alunoIds })).Status);
    }

    [Fact]
    public async Task Recusa_Aluno_Nao_Inscrito_No_Time()
    {
        InscreverAlunosDeCadaPosicao(1);
        var naoInscrito = CriarEInscrever(_sub11, PosicaoJogador.Pivo, inscrever: false);
        var alunoIds = _alunoRepositorio.Alunos.Where(aluno => aluno.Posicao != PosicaoJogador.Pivo || aluno.Id == naoInscrito.Id).Select(aluno => aluno.Id).ToList();
        alunoIds.Remove(_alunoRepositorio.Alunos.First(aluno => aluno.Posicao == PosicaoJogador.Pivo && aluno.Id != naoInscrito.Id).Id);

        Assert.False((await _servico.CriarEquipeAsync(_campeonato.Id, new CriarEquipeRequestDTO { AlunoIds = alunoIds })).Status);
    }

    [Fact]
    public async Task Sorteio_Cria_Os_Times_E_Associa_Os_Alunos()
    {
        InscreverAlunosDeCadaPosicao(5);

        var resultado = await _servico.SortearEquipesAsync(_campeonato.Id, new SortearEquipesRequestDTO());

        Assert.True(resultado.Status);
        Assert.Equal(5, resultado.Data!.Count);
        Assert.All(_competicaoRepositorio.Participacoes, participacao => Assert.NotNull(participacao.EquipeId));
    }

    [Fact]
    public async Task Sorteio_Falha_Sem_Alunos_Suficientes()
    {
        CriarEInscrever(_sub11, PosicaoJogador.Goleiro);

        Assert.False((await _servico.SortearEquipesAsync(_campeonato.Id, new SortearEquipesRequestDTO())).Status);
    }

    [Fact]
    public async Task Nao_Remove_Inscricao_De_Aluno_Que_Esta_Em_Time()
    {
        InscreverAlunosDeCadaPosicao(1);
        await _servico.SortearEquipesAsync(_campeonato.Id, new SortearEquipesRequestDTO());

        var resultado = await _servico.RemoverParticipacaoAsync(_campeonato.Id, _competicaoRepositorio.Participacoes[0].Id);

        Assert.False(resultado.Status);
    }

    [Fact]
    public async Task Remover_Time_Libera_Os_Alunos()
    {
        InscreverAlunosDeCadaPosicao(1);
        await _servico.SortearEquipesAsync(_campeonato.Id, new SortearEquipesRequestDTO());
        var equipeId = _competicaoRepositorio.Equipes[0].Id;

        var resultado = await _servico.RemoverEquipeAsync(_campeonato.Id, equipeId);

        Assert.True(resultado.Status);
        Assert.Empty(_competicaoRepositorio.Equipes);
        Assert.All(_competicaoRepositorio.Participacoes, participacao => Assert.Null(participacao.EquipeId));
    }

    [Fact]
    public async Task Gerar_Jogos_Exige_Dois_Times()
    {
        InscreverAlunosDeCadaPosicao(1);
        await _servico.SortearEquipesAsync(_campeonato.Id, new SortearEquipesRequestDTO());

        Assert.False((await _servico.GerarJogosAsync(_campeonato.Id)).Status);
    }

    [Fact]
    public async Task Tres_Times_Geram_Dois_Jogos_Normais_E_Um_Bye()
    {
        var jogos = await GerarJogosComTimes(3);

        Assert.Equal(3, jogos.Count);
        Assert.Equal(1, jogos.Count(jogo => jogo.Bye));
    }

    [Fact]
    public async Task Nao_Gera_Jogos_Duas_Vezes_Nem_Cria_Time_Depois()
    {
        await GerarJogosComTimes(2);

        Assert.False((await _servico.GerarJogosAsync(_campeonato.Id)).Status);
        Assert.False((await _servico.SortearEquipesAsync(_campeonato.Id, new SortearEquipesRequestDTO())).Status);
    }

    [Fact]
    public async Task Estatisticas_Listam_Os_Dez_Jogadores()
    {
        var jogos = await GerarJogosComTimes(2);

        var resultado = await _servico.ObterEstatisticasAsync(jogos[0].Id);

        Assert.Equal(10, resultado.Data!.Jogadores.Count);
    }

    [Fact]
    public async Task Placar_E_A_Soma_Dos_Gols_Dos_Jogadores()
    {
        var jogos = await GerarJogosComTimes(2);

        await Jogar(jogos[0], 3, 1);

        var atualizado = (await _servico.ListarJogosAsync(_campeonato.Id)).Data![0];
        Assert.Equal(3, atualizado.GolsA);
        Assert.Equal(1, atualizado.GolsB);
    }

    [Fact]
    public async Task Nao_Finaliza_Jogo_Empatado()
    {
        var jogos = await GerarJogosComTimes(2);
        await Jogar(jogos[0], 1, 1);

        var resultado = await _servico.FinalizarJogoAsync(jogos[0].Id);

        Assert.False(resultado.Status);
        Assert.Contains("gol de ouro", resultado.Message);
    }

    [Fact]
    public async Task Finaliza_Jogo_Com_Vencedor_E_Fecha_O_Campeonato_Na_Final()
    {
        var jogos = await GerarJogosComTimes(2);
        await Jogar(jogos[0], 0, 2);

        var resultado = await _servico.FinalizarJogoAsync(jogos[0].Id);
        var ranking = await _servico.ObterRankingAsync(_campeonato.Id);

        Assert.True(resultado.Status);
        Assert.True(ranking.Data!.Finalizado);
        Assert.NotNull(ranking.Data.CampeaNome);
    }

    [Fact]
    public async Task Jogo_Finalizado_Nao_Aceita_Novas_Estatisticas()
    {
        var jogos = await GerarJogosComTimes(2);
        await Jogar(jogos[0], 1, 0);
        await _servico.FinalizarJogoAsync(jogos[0].Id);

        var resultado = await _servico.SalvarEstatisticasAsync(jogos[0].Id, new SalvarEstatisticasRequestDTO());

        Assert.False(resultado.Status);
    }

    [Fact]
    public async Task Vencedor_Da_Semifinal_Chega_Na_Final()
    {
        var jogos = await GerarJogosComTimes(4);
        var semifinais = jogos.Where(jogo => jogo.Fase == 1).ToList();

        await Jogar(semifinais[0], 2, 0);
        await _servico.FinalizarJogoAsync(semifinais[0].Id);
        await Jogar(semifinais[1], 0, 1);
        await _servico.FinalizarJogoAsync(semifinais[1].Id);

        var final = (await _servico.ListarJogosAsync(_campeonato.Id)).Data!.First(jogo => jogo.Fase == 2);
        Assert.Equal("Pendente", final.Status);
        Assert.NotNull(final.EquipeANome);
        Assert.NotNull(final.EquipeBNome);
    }

    [Fact]
    public async Task Recusa_Estatistica_Negativa_Acima_Do_Limite_E_Defesa_De_Quem_Nao_E_Goleiro()
    {
        var jogos = await GerarJogosComTimes(2);
        var jogadores = (await _servico.ObterEstatisticasAsync(jogos[0].Id)).Data!.Jogadores;
        var fixo = jogadores.First(jogador => jogador.Posicao == (int)PosicaoJogador.Fixo);

        var negativa = await _servico.SalvarEstatisticasAsync(jogos[0].Id, new SalvarEstatisticasRequestDTO
        { Jogadores = new() { new EstatisticaDoJogadorRequestDTO { AlunoId = fixo.AlunoId, Gols = -1 } } });
        var acimaDoLimite = await _servico.SalvarEstatisticasAsync(jogos[0].Id, new SalvarEstatisticasRequestDTO
        { Jogadores = new() { new EstatisticaDoJogadorRequestDTO { AlunoId = fixo.AlunoId, Faltas = 1000 } } });
        var defesaDeLinha = await _servico.SalvarEstatisticasAsync(jogos[0].Id, new SalvarEstatisticasRequestDTO
        { Jogadores = new() { new EstatisticaDoJogadorRequestDTO { AlunoId = fixo.AlunoId, Defesas = 2 } } });

        Assert.False(negativa.Status);
        Assert.False(acimaDoLimite.Status);
        Assert.False(defesaDeLinha.Status);
    }

    [Fact]
    public async Task Recusa_Jogador_De_Fora_Do_Jogo_E_Jogador_Repetido()
    {
        var jogos = await GerarJogosComTimes(2);
        var alunoDeFora = CriarEInscrever(_sub11, PosicaoJogador.Pivo, inscrever: false);
        var jogador = (await _servico.ObterEstatisticasAsync(jogos[0].Id)).Data!.Jogadores[0];

        var deFora = await _servico.SalvarEstatisticasAsync(jogos[0].Id, new SalvarEstatisticasRequestDTO
        { Jogadores = new() { new EstatisticaDoJogadorRequestDTO { AlunoId = alunoDeFora.Id } } });
        var repetido = await _servico.SalvarEstatisticasAsync(jogos[0].Id, new SalvarEstatisticasRequestDTO
        { Jogadores = new() { new EstatisticaDoJogadorRequestDTO { AlunoId = jogador.AlunoId }, new EstatisticaDoJogadorRequestDTO { AlunoId = jogador.AlunoId } } });

        Assert.False(deFora.Status);
        Assert.False(repetido.Status);
    }

    [Fact]
    public async Task Ranking_Inclui_Todos_Os_Inscritos_E_Coloca_O_Artilheiro_Em_Primeiro()
    {
        var jogos = await GerarJogosComTimes(2);
        await Jogar(jogos[0], 4, 0);
        await _servico.FinalizarJogoAsync(jogos[0].Id);

        var ranking = (await _servico.ObterRankingAsync(_campeonato.Id)).Data!;

        Assert.Equal(10, ranking.Itens.Count);
        Assert.Equal(1, ranking.Itens[0].Colocacao);
        Assert.Equal(4, ranking.Itens[0].Gols);
    }

    [Fact]
    public async Task Nao_Reinicia_Jogos_Com_Jogo_Finalizado_Mas_Reinicia_Sem_Nenhum()
    {
        var jogos = await GerarJogosComTimes(4);

        var semNadaFinalizado = await _servico.ReiniciarJogosAsync(_campeonato.Id);
        Assert.True(semNadaFinalizado.Status);

        var novosJogos = (await _servico.GerarJogosAsync(_campeonato.Id)).Data!;
        await Jogar(novosJogos.First(jogo => jogo.Fase == 1), 1, 0);
        await _servico.FinalizarJogoAsync(novosJogos.First(jogo => jogo.Fase == 1).Id);

        Assert.False((await _servico.ReiniciarJogosAsync(_campeonato.Id)).Status);
        Assert.Equal(3, jogos.Count);
    }

    [Fact]
    public async Task Campeonato_Inexistente_Retorna_Nao_Encontrado()
    {
        var resultado = await _servico.ListarEquipesAsync(Guid.NewGuid());

        Assert.False(resultado.Status);
        Assert.True(resultado.NaoEncontrado);
    }
}
