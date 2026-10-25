using Domain.Utilitarios;

namespace Tests;

public class DiasDaSemanaTest
{
    [Theory]
    [InlineData("Terça,Quinta")]
    [InlineData("terca, quinta")]
    [InlineData("Segunda")]
    [InlineData("Quarta-feira,Sexta-feira")]
    [InlineData("Segunda,Terça,Quarta,Quinta,Sexta,Sábado")]
    public void Deve_Aceitar_Dias_Validos(string texto)
    {
        Assert.True(DiasDaSemana.EhValido(texto));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Seg/Qua")]
    [InlineData("Terça e Quinta")]
    [InlineData("Terça,Domingo")]
    [InlineData("Terça,")]
    [InlineData("Banana")]
    public void Deve_Recusar_Dias_Invalidos_Ou_Em_Formato_Diferente_Do_Padrao(string? texto)
    {
        Assert.False(DiasDaSemana.EhValido(texto));
    }

    [Theory]
    [InlineData("Terça e Quinta", "Terça,Quinta")]
    [InlineData("Seg/Qua", "Segunda,Quarta")]
    [InlineData("Quinta,Terça,Terça", "Terça,Quinta")]
    [InlineData("Quarta-feira", "Quarta")]
    [InlineData("Sábado", "Sábado")]
    public void Deve_Padronizar_Texto_Antigo_Para_O_Formato_Novo(string texto, string esperado)
    {
        Assert.Equal(esperado, DiasDaSemana.Padronizar(texto));
    }

    [Fact]
    public void Deve_Converter_Texto_Em_Dias_Da_Semana_Do_Dotnet()
    {
        var dias = DiasDaSemana.Converter("Terça,Quinta");

        Assert.Equal(new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }, dias);
    }

    [Fact]
    public void Deve_Devolver_Lista_Vazia_Quando_Nao_Reconhecer_Nenhum_Dia()
    {
        Assert.Empty(DiasDaSemana.Converter("A definir"));
    }

    [Fact]
    public void Deve_Formatar_Dias_Na_Ordem_Da_Semana()
    {
        var texto = DiasDaSemana.Formatar(new[] { DayOfWeek.Friday, DayOfWeek.Monday });

        Assert.Equal("Segunda,Sexta", texto);
    }
}