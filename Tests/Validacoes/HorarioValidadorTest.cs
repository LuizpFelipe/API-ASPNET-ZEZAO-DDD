using Application.Validacoes;

namespace Tests;

public class HorarioValidadorTest
{
    [Theory]
    [InlineData("17h")]
    [InlineData("17h30")]
    [InlineData("9:00")]
    [InlineData("09:05")]
    [InlineData(" 8h ")]
    public void Deve_Aceitar_Horario_Valido(string horario)
    {
        Assert.True(HorarioValidador.EhValido(horario));
    }

    [Theory]
    [InlineData("")]
    [InlineData("tarde")]
    [InlineData("24h")]
    [InlineData("17h75")]
    [InlineData("17")]
    [InlineData("Segunda 17h")]
    public void Deve_Recusar_Horario_Invalido(string horario)
    {
        Assert.False(HorarioValidador.EhValido(horario));
    }
}
