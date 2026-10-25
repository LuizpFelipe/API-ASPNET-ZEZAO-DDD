using Application.Validacoes;

namespace Tests;

public class CpfValidadorTest
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData(" 529.982.247-25 ")]
    public void Deve_Aceitar_Cpf_Valido_Com_Ou_Sem_Mascara(string cpf)
    {
        Assert.True(CpfValidador.EhValido(cpf));
    }

    [Theory]
    [InlineData("52998224726")]
    [InlineData("529.982.247-24")]
    [InlineData("123")]
    [InlineData("529982247250")]
    [InlineData("abcdefghijk")]
    public void Deve_Recusar_Cpf_Invalido(string cpf)
    {
        Assert.False(CpfValidador.EhValido(cpf));
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("111.111.111-11")]
    [InlineData("99999999999")]
    public void Deve_Recusar_Sequencia_De_Digitos_Repetidos(string cpf)
    {
        Assert.False(CpfValidador.EhValido(cpf));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Recusar_Cpf_Vazio(string? cpf)
    {
        Assert.False(CpfValidador.EhValido(cpf));
    }

    [Fact]
    public void Deve_Normalizar_Removendo_Tudo_Que_Nao_For_Digito()
    {
        Assert.Equal("52998224725", CpfValidador.Normalizar("529.982.247-25"));
        Assert.Equal(string.Empty, CpfValidador.Normalizar(null));
    }

    [Fact]
    public void Deve_Formatar_Cpf_Com_Mascara()
    {
        Assert.Equal("529.982.247-25", CpfValidador.Formatar("52998224725"));
    }

    [Fact]
    public void Nao_Deve_Formatar_Cpf_Incompleto()
    {
        Assert.Equal("5299", CpfValidador.Formatar("5299"));
    }
}