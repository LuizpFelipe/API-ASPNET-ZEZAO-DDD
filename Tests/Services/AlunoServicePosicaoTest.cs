using Application.Request;
using Application.Services;
using Domain.Enums;

namespace Tests;

public class AlunoServicePosicaoTest
{
    // As validações de posição acontecem antes de qualquer acesso aos repositórios, por isso as dependências ficam nulas.
    private readonly AlunoService _servico = new(null!, null!, null!, null!, null!);

    private static CriarAlunoRequestDTO CriarRequisicao(PosicaoJogador? posicao) => new()
    {
        NomeCompleto = "Enzo Ferreira",
        DataNascimento = new DateOnly(2015, 3, 1),
        NivelTecnico = NivelTecnico.Iniciante,
        ResponsavelNome = "Ana Ferreira",
        Telefone = "44999990000",
        Endereco = "Rua A, 10",
        TurmaId = Guid.NewGuid(),
        ResponsavelCpf = "12345678909",
        Posicao = posicao
    };

    [Fact]
    public async Task Cadastro_Exige_A_Posicao_Do_Aluno()
    {
        var resultado = await _servico.CriarAsync(CriarRequisicao(null));

        Assert.False(resultado.Status);
        Assert.Contains("posição", resultado.Message);
    }

    [Fact]
    public async Task Cadastro_Recusa_Posicao_Fora_Da_Lista()
    {
        var resultado = await _servico.CriarAsync(CriarRequisicao((PosicaoJogador)99));

        Assert.False(resultado.Status);
    }
}
