using Domain.Entities;

namespace Tests;

public class TurmaTemAulaEmTest
{
    private static Turma CriarTurma(string diasSemana) =>
        new Turma("Turma A", diasSemana, "18h", Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Deve_Ter_Aula_Nos_Dias_Da_Turma()
    {
        var turma = CriarTurma("Terça,Quinta");

        Assert.True(turma.TemAulaEm(new DateOnly(2026, 10, 27)));
        Assert.True(turma.TemAulaEm(new DateOnly(2026, 10, 29)));
    }

    [Fact]
    public void Nao_Deve_Ter_Aula_Em_Outros_Dias()
    {
        var turma = CriarTurma("Terça,Quinta");

        Assert.False(turma.TemAulaEm(new DateOnly(2026, 10, 28)));
        Assert.False(turma.TemAulaEm(new DateOnly(2026, 10, 25)));
    }

    [Fact]
    public void Deve_Entender_Texto_Antigo_Ja_Gravado()
    {
        var turma = CriarTurma("Terça e Quinta");

        Assert.True(turma.TemAulaEm(new DateOnly(2026, 10, 27)));
    }
}