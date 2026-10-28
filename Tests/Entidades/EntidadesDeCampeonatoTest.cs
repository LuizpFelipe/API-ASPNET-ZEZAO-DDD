using Domain.Entities;
using Domain.Enums;

namespace Tests;

public class EntidadesDeCampeonatoTest
{
    [Fact]
    public void Jogo_Com_Os_Dois_Times_Fica_Pendente()
    {
        var jogo = new JogoCampeonato(Guid.NewGuid(), 1, 0);
        Assert.Equal(StatusJogo.Aguardando, jogo.Status);

        jogo.DefinirEquipe(true, Guid.NewGuid());
        Assert.Equal(StatusJogo.Aguardando, jogo.Status);
        jogo.DefinirEquipe(false, Guid.NewGuid());

        Assert.Equal(StatusJogo.Pendente, jogo.Status);
    }

    [Fact]
    public void Finalizar_Define_O_Vencedor()
    {
        var jogo = new JogoCampeonato(Guid.NewGuid(), 1, 0);
        var vencedora = Guid.NewGuid();

        jogo.Finalizar(vencedora);

        Assert.Equal(StatusJogo.Finalizado, jogo.Status);
        Assert.Equal(vencedora, jogo.EquipeVencedoraId);
    }

    [Fact]
    public void Bye_Finaliza_O_Jogo_Marcando_Como_Bye()
    {
        var jogo = new JogoCampeonato(Guid.NewGuid(), 1, 0);

        jogo.FinalizarPorBye(Guid.NewGuid());

        Assert.True(jogo.Bye);
        Assert.Equal(StatusJogo.Finalizado, jogo.Status);
    }

    [Fact]
    public void Atualizar_Placar_Guarda_Os_Gols()
    {
        var jogo = new JogoCampeonato(Guid.NewGuid(), 1, 0);

        jogo.AtualizarPlacar(3, 2);

        Assert.Equal(3, jogo.GolsA);
        Assert.Equal(2, jogo.GolsB);
    }

    [Fact]
    public void Participacao_Guarda_E_Libera_O_Time()
    {
        var participacao = new ParticipacaoCampeonato(Guid.NewGuid(), Guid.NewGuid(), null);
        var equipeId = Guid.NewGuid();

        participacao.DefinirEquipe(equipeId);
        Assert.Equal(equipeId, participacao.EquipeId);
        participacao.DefinirEquipe(null);

        Assert.Null(participacao.EquipeId);
    }

    [Fact]
    public void Aluno_Guarda_A_Posicao_E_Atualizar_Sem_Posicao_Mantem_A_Anterior()
    {
        var categoriaId = Guid.NewGuid();
        var turmaId = Guid.NewGuid();
        var aluno = new Aluno("Enzo Ferreira", new DateOnly(2015, 3, 1), NivelTecnico.Iniciante, null, "Ana", "12345678909", "44999990000", "Rua A", turmaId, categoriaId, PosicaoJogador.Fixo);

        aluno.AtualizarDados("Enzo Ferreira", new DateOnly(2015, 3, 1), NivelTecnico.Iniciante, null, "Ana", "12345678909", "44999990000", "Rua A", turmaId, categoriaId);
        Assert.Equal(PosicaoJogador.Fixo, aluno.Posicao);

        aluno.AtualizarDados("Enzo Ferreira", new DateOnly(2015, 3, 1), NivelTecnico.Iniciante, null, "Ana", "12345678909", "44999990000", "Rua A", turmaId, categoriaId, PosicaoJogador.Pivo);
        Assert.Equal(PosicaoJogador.Pivo, aluno.Posicao);
    }
}
