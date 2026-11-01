using Application.Interfaces;
using Application.Request;
using Application.Response;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Tests.Fakes;

namespace Tests;

public class InativacaoDeAlunoTest
{
    private static Aluno CriarAluno() => new("Enzo Ferreira", new DateOnly(2015, 3, 1), NivelTecnico.Iniciante,
        null, "Ana Ferreira", "52998224725", "44999990000", "Rua A, 10", Guid.NewGuid(), Guid.NewGuid(), PosicaoJogador.Goleiro);

    private sealed class ContratoServiceCapturando : IContratoService
    {
        public List<Guid> AlunosComContratosCancelados { get; } = new();

        public Task<int> CancelarContratosAtivosDoAlunoAsync(Guid alunoId)
        {
            AlunosComContratosCancelados.Add(alunoId);
            return Task.FromResult(1);
        }

        public Task<ContratoResponseDTO> CriarAsync(Guid alunoId) => throw new NotImplementedException();
        public Task<ContratoResponseDTO> ListarAsync() => throw new NotImplementedException();
        public Task<ContratoResponseDTO> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ContratoResponseDTO> AtualizarAsync(Guid id, AtualizarContratoRequestDTO dto) => throw new NotImplementedException();
        public Task<ContratoResponseDTO> GerarPdfAsync(Guid id) => throw new NotImplementedException();
    }

    private sealed class ContratoRepositorioEmMemoria : IContratoRepository
    {
        public List<Contrato> Contratos { get; } = new();

        public Task<IEnumerable<Contrato>> ObterTodosAsync() => Task.FromResult<IEnumerable<Contrato>>(Contratos.ToList());
        public Task<Contrato?> ObterPorIdAsync(Guid id) => Task.FromResult(Contratos.FirstOrDefault(contrato => contrato.Id == id));
        public Task AdicionarAsync(Contrato contrato) { Contratos.Add(contrato); return Task.CompletedTask; }
        public Task AtualizarAsync(Contrato contrato) => Task.CompletedTask;
        public Task<bool> ExisteContratoAtivoDoAlunoAsync(Guid alunoId) =>
            Task.FromResult(Contratos.Any(contrato => contrato.AlunoId == alunoId && contrato.Status == StatusContrato.Ativo));
        public Task<IEnumerable<Contrato>> ObterAtivosDoAlunoAsync(Guid alunoId) =>
            Task.FromResult<IEnumerable<Contrato>>(Contratos.Where(contrato => contrato.AlunoId == alunoId && contrato.Status == StatusContrato.Ativo).ToList());
    }

    [Fact]
    public async Task Inativar_Aluno_Cancela_Os_Contratos_Ativos_Dele()
    {
        var alunos = new AlunoRepositorioEmMemoria();
        var aluno = CriarAluno();
        alunos.Alunos.Add(aluno);
        var contratos = new ContratoServiceCapturando();
        var servico = new AlunoService(alunos, null!, null!, contratos, null!);

        var resultado = await servico.RemoverAsync(aluno.Id);

        Assert.True(resultado.Status);
        Assert.Equal(StatusAluno.Inativo, aluno.Status);
        Assert.Equal(new[] { aluno.Id }, contratos.AlunosComContratosCancelados);
    }

    [Fact]
    public async Task Inativar_Aluno_Inexistente_Nao_Mexe_Em_Contrato()
    {
        var contratos = new ContratoServiceCapturando();
        var servico = new AlunoService(new AlunoRepositorioEmMemoria(), null!, null!, contratos, null!);

        var resultado = await servico.RemoverAsync(Guid.NewGuid());

        Assert.False(resultado.Status);
        Assert.Empty(contratos.AlunosComContratosCancelados);
    }

    [Fact]
    public async Task Cancelar_Contratos_Do_Aluno_Muda_Apenas_Os_Ativos_Dele()
    {
        var alunoId = Guid.NewGuid();
        var outroAlunoId = Guid.NewGuid();
        var repositorio = new ContratoRepositorioEmMemoria();
        var ativo = new Contrato(alunoId, new DateOnly(2026, 10, 1), new DateOnly(2027, 10, 1), 150m, 12);
        var jaCancelado = new Contrato(alunoId, new DateOnly(2025, 10, 1), new DateOnly(2026, 10, 1), 150m, 12);
        jaCancelado.Cancelar();
        var deOutroAluno = new Contrato(outroAlunoId, new DateOnly(2026, 10, 1), new DateOnly(2027, 10, 1), 150m, 12);
        repositorio.Contratos.AddRange(new[] { ativo, jaCancelado, deOutroAluno });
        var servico = new ContratoService(repositorio, null!, null!, null!, null!, null!, null!);

        var cancelados = await servico.CancelarContratosAtivosDoAlunoAsync(alunoId);

        Assert.Equal(1, cancelados);
        Assert.Equal(StatusContrato.Cancelado, ativo.Status);
        Assert.Equal(StatusContrato.Ativo, deOutroAluno.Status);
    }

    private sealed class UnidadeDeTrabalhoDeTeste : IUnitOfWork
    {
        public Task ExecutarEmTransacaoAsync(Func<Task> acao) => acao();
    }

    private sealed class ContratoServiceQueCria : IContratoService
    {
        public List<Guid> ContratosCriadosPara { get; } = new();
        public List<Guid> ContratosCanceladosPara { get; } = new();

        public Task<ContratoResponseDTO> CriarAsync(Guid alunoId)
        {
            ContratosCriadosPara.Add(alunoId);
            return Task.FromResult(new ContratoResponseDTO { Status = true });
        }

        public Task<int> CancelarContratosAtivosDoAlunoAsync(Guid alunoId)
        {
            ContratosCanceladosPara.Add(alunoId);
            return Task.FromResult(0);
        }

        public Task<ContratoResponseDTO> ListarAsync() => throw new NotImplementedException();
        public Task<ContratoResponseDTO> ObterPorIdAsync(Guid id) => throw new NotImplementedException();
        public Task<ContratoResponseDTO> AtualizarAsync(Guid id, AtualizarContratoRequestDTO dto) => throw new NotImplementedException();
        public Task<ContratoResponseDTO> GerarPdfAsync(Guid id) => throw new NotImplementedException();
    }

    [Fact]
    public async Task Cadastrar_Aluno_Antes_Inativado_Reativa_O_Cadastro_E_Cria_Novo_Contrato()
    {
        var categoria = new Categoria("Sub-11", 9, 10);
        var turma = new Turma("Sub-11 - Sexta 14h", "Sexta", "14h", categoria.Id, Guid.NewGuid());
        var nascimento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10).AddDays(-30));
        var inativo = new Aluno("Enzo Ferreira", nascimento, NivelTecnico.Iniciante, null, "Ana", "52998224725", "44999990000", "Rua A", turma.Id, categoria.Id, PosicaoJogador.Fixo);
        inativo.Desativar();

        var alunos = new AlunoRepositorioEmMemoria();
        alunos.Alunos.Add(inativo);
        var categorias = new CategoriaRepositorioEmMemoria();
        categorias.Categorias.Add(categoria);
        var turmas = new TurmaRepositorioEmMemoria();
        turmas.Turmas.Add(turma);
        var contratos = new ContratoServiceQueCria();
        var servico = new AlunoService(alunos, categorias, turmas, contratos, new UnidadeDeTrabalhoDeTeste());

        var resultado = await servico.CriarAsync(new CriarAlunoRequestDTO
        {
            NomeCompleto = "Enzo Ferreira",
            DataNascimento = nascimento,
            NivelTecnico = NivelTecnico.Intermediario,
            ResponsavelNome = "Ana Ferreira",
            Telefone = "44988887777",
            Endereco = "Rua B, 20",
            TurmaId = turma.Id,
            ResponsavelCpf = "52998224725",
            Posicao = PosicaoJogador.Pivo
        });

        Assert.True(resultado.Status, resultado.Message);
        Assert.Single(alunos.Alunos);
        Assert.Equal(StatusAluno.Ativo, inativo.Status);
        Assert.Equal("44988887777", inativo.Telefone);
        Assert.Equal(new[] { inativo.Id }, contratos.ContratosCriadosPara);
        Assert.Equal(new[] { inativo.Id }, contratos.ContratosCanceladosPara);
    }
}
