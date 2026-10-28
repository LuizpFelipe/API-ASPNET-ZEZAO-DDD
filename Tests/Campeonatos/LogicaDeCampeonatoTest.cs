using Application.Campeonatos;
using Domain.Entities;
using Domain.Enums;

namespace Tests;

public class LogicaDeCampeonatoTest
{
    private static List<CandidatoAoSorteio> CriarCandidatos(int quantidadePorPosicao)
    {
        var candidatos = new List<CandidatoAoSorteio>();
        foreach (var posicao in NomesDePosicao.TodasAsPosicoes)
            for (var indice = 0; indice < quantidadePorPosicao; indice++)
                candidatos.Add(new CandidatoAoSorteio(Guid.NewGuid(), posicao));
        return candidatos;
    }

    [Fact]
    public void Sorteio_Forma_Times_Com_Uma_Posicao_De_Cada()
    {
        var candidatos = CriarCandidatos(5);

        var resultado = SorteioDeEquipes.Sortear(candidatos, null, new Random(1));

        Assert.Null(resultado.Erro);
        Assert.Equal(5, resultado.Equipes.Count);
        foreach (var equipe in resultado.Equipes)
        {
            var posicoes = equipe.Select(alunoId => candidatos.First(candidato => candidato.AlunoId == alunoId).Posicao).ToList();
            Assert.Equal(5, posicoes.Distinct().Count());
        }
    }

    [Fact]
    public void Sorteio_Nao_Repete_Aluno_Entre_Times()
    {
        var resultado = SorteioDeEquipes.Sortear(CriarCandidatos(4), null, new Random(2));

        var todosOsAlunos = resultado.Equipes.SelectMany(equipe => equipe).ToList();
        Assert.Equal(todosOsAlunos.Count, todosOsAlunos.Distinct().Count());
    }

    [Fact]
    public void Sorteio_Respeita_A_Quantidade_Pedida_E_Devolve_As_Sobras()
    {
        var resultado = SorteioDeEquipes.Sortear(CriarCandidatos(4), 2, new Random(3));

        Assert.Equal(2, resultado.Equipes.Count);
        Assert.Equal(10, resultado.Sobras.Count);
    }

    [Fact]
    public void Sorteio_Recusa_Quantidade_Maior_Que_O_Possivel()
    {
        var resultado = SorteioDeEquipes.Sortear(CriarCandidatos(2), 3, new Random(4));

        Assert.NotNull(resultado.Erro);
        Assert.Empty(resultado.Equipes);
    }

    [Fact]
    public void Sorteio_Recusa_Quando_Falta_Uma_Posicao()
    {
        var candidatos = CriarCandidatos(3).Where(candidato => candidato.Posicao != PosicaoJogador.Goleiro).ToList();

        var resultado = SorteioDeEquipes.Sortear(candidatos, null, new Random(5));

        Assert.NotNull(resultado.Erro);
        Assert.Contains("Goleiro", resultado.Erro);
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 4)]
    public void Numero_De_Fases_Cresce_Com_A_Quantidade_De_Times(int quantidadeDeEquipes, int fasesEsperadas)
    {
        Assert.Equal(fasesEsperadas, ChaveamentoMataMata.NumeroDeFases(quantidadeDeEquipes));
    }

    [Fact]
    public void Nomes_Das_Fases_Terminam_Em_Final()
    {
        Assert.Equal("Final", ChaveamentoMataMata.NomeDaFase(3, 3));
        Assert.Equal("Semifinal", ChaveamentoMataMata.NomeDaFase(2, 3));
        Assert.Equal("Quartas de final", ChaveamentoMataMata.NomeDaFase(1, 3));
    }

    [Fact]
    public void Chaveamento_Com_4_Times_Tem_3_Jogos_E_Todos_Os_Times_Na_Primeira_Fase()
    {
        var equipes = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList();

        var jogos = ChaveamentoMataMata.Gerar(equipes, new Random(1));

        Assert.Equal(3, jogos.Count);
        var primeiraFase = jogos.Where(jogo => jogo.Fase == 1).ToList();
        Assert.Equal(2, primeiraFase.Count);
        Assert.Equal(4, primeiraFase.SelectMany(jogo => new[] { jogo.EquipeAId, jogo.EquipeBId }).Where(equipe => equipe.HasValue).Distinct().Count());
    }

    [Fact]
    public void Chaveamento_Com_3_Times_Da_Bye_Para_Um_Time()
    {
        var equipes = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToList();

        var jogos = ChaveamentoMataMata.Gerar(equipes, new Random(1));

        var primeiraFase = jogos.Where(jogo => jogo.Fase == 1).ToList();
        Assert.Equal(1, primeiraFase.Count(jogo => jogo.EquipeAId == null || jogo.EquipeBId == null));
    }

    [Fact]
    public void Chaveamento_Exige_Ao_Menos_Dois_Times()
    {
        Assert.Throws<ArgumentException>(() => ChaveamentoMataMata.Gerar(new List<Guid> { Guid.NewGuid() }, new Random(1)));
    }

    [Fact]
    public void Vencedor_Avanca_Para_O_Lado_Correto_Da_Proxima_Fase()
    {
        var campeonatoId = Guid.NewGuid();
        var primeiroJogo = new JogoCampeonato(campeonatoId, 1, 0);
        var segundoJogo = new JogoCampeonato(campeonatoId, 1, 1);
        var final = new JogoCampeonato(campeonatoId, 2, 0);
        var jogos = new List<JogoCampeonato> { primeiroJogo, segundoJogo, final };
        var vencedoraDoPrimeiro = Guid.NewGuid();
        var vencedoraDoSegundo = Guid.NewGuid();

        primeiroJogo.Finalizar(vencedoraDoPrimeiro);
        segundoJogo.Finalizar(vencedoraDoSegundo);
        ChaveamentoMataMata.Avancar(jogos, primeiroJogo);
        Assert.Equal(StatusJogo.Aguardando, final.Status);
        ChaveamentoMataMata.Avancar(jogos, segundoJogo);

        Assert.Equal(vencedoraDoPrimeiro, final.EquipeAId);
        Assert.Equal(vencedoraDoSegundo, final.EquipeBId);
        Assert.Equal(StatusJogo.Pendente, final.Status);
    }

    [Fact]
    public void Final_Nao_Tem_Proximo_Jogo()
    {
        var final = new JogoCampeonato(Guid.NewGuid(), 2, 0);
        final.Finalizar(Guid.NewGuid());

        Assert.False(ChaveamentoMataMata.Avancar(new List<JogoCampeonato> { final }, final));
    }

    [Fact]
    public void Ranking_Aplica_Os_Pesos_E_Ordena_Por_Pontos()
    {
        var artilheiro = Guid.NewGuid();
        var garcom = Guid.NewGuid();
        var jogoId = Guid.NewGuid();
        var estatisticas = new List<EstatisticaJogo>
        {
            new(jogoId, artilheiro, 2, 0, 10, 0, 0, 1, 0, 0),
            new(jogoId, garcom, 0, 2, 0, 1, 0, 0, 1, 0)
        };

        var ranking = CalculadoraDeRanking.Calcular(estatisticas, new[] { garcom, artilheiro });

        Assert.Equal(artilheiro, ranking[0].AlunoId);
        Assert.Equal(10m, ranking[0].Pontos);
        Assert.Equal(garcom, ranking[1].AlunoId);
        Assert.Equal(5m, ranking[1].Pontos);
    }

    [Fact]
    public void Ranking_Inclui_Participante_Sem_Estatistica_Com_Zero_Pontos()
    {
        var semJogos = Guid.NewGuid();

        var ranking = CalculadoraDeRanking.Calcular(new List<EstatisticaJogo>(), new[] { semJogos });

        Assert.Single(ranking);
        Assert.Equal(0m, ranking[0].Pontos);
        Assert.Equal(0, ranking[0].Jogos);
    }

    [Fact]
    public void Ranking_Desempata_Por_Gols()
    {
        var comGols = Guid.NewGuid();
        var comPasses = Guid.NewGuid();
        var jogoId = Guid.NewGuid();
        var estatisticas = new List<EstatisticaJogo>
        {
            new(jogoId, comGols, 1, 0, 0, 0, 0, 0, 0, 0),
            new(jogoId, comPasses, 0, 0, 50, 0, 0, 0, 0, 0)
        };

        var ranking = CalculadoraDeRanking.Calcular(estatisticas, new[] { comPasses, comGols });

        Assert.Equal(5m, ranking[0].Pontos);
        Assert.Equal(5m, ranking[1].Pontos);
        Assert.Equal(comGols, ranking[0].AlunoId);
    }

    [Fact]
    public void Elegibilidade_Inclui_A_Propria_Categoria_E_A_Imediatamente_Abaixo()
    {
        var sub9 = new Categoria("Sub-9", 7, 8);
        var sub10 = new Categoria("Sub-10", 9, 10);
        var sub11 = new Categoria("Sub-11", 11, 12);
        var todas = new List<Categoria> { sub9, sub10, sub11 };

        var elegiveis = ElegibilidadeDeCategoria.ObterCategoriasElegiveis(sub11, todas);

        Assert.Contains(sub11.Id, elegiveis);
        Assert.Contains(sub10.Id, elegiveis);
        Assert.DoesNotContain(sub9.Id, elegiveis);
    }

    [Fact]
    public void Elegibilidade_Da_Categoria_Mais_Nova_Tem_So_Ela_Mesma()
    {
        var sub9 = new Categoria("Sub-9", 7, 8);
        var sub10 = new Categoria("Sub-10", 9, 10);

        var elegiveis = ElegibilidadeDeCategoria.ObterCategoriasElegiveis(sub9, new List<Categoria> { sub9, sub10 });

        Assert.Single(elegiveis);
    }

    [Fact]
    public void Nomes_De_Posicao_Cobrem_As_Cinco_Posicoes()
    {
        Assert.Equal(5, NomesDePosicao.TodasAsPosicoes.Length);
        Assert.Equal("Pivô", NomesDePosicao.Obter(PosicaoJogador.Pivo));
        Assert.Null(NomesDePosicao.Obter((PosicaoJogador?)null));
    }
}
