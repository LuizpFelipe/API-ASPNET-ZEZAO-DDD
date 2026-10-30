using System.Reflection;
using Application.Configuration;
using Application.Interfaces;
using Application.Response;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Tests.Services;

public class RotinaFinanceiraServiceTest
{
    private static readonly DateOnly Hoje = new(2026, 10, 30);

    private readonly ParcelaRepositorioDeTeste _parcelas = new();
    private readonly AlunoRepositorioDeTeste _alunos = new();
    private readonly AvisoRepositorioDeTeste _avisos = new();
    private readonly FinanceiroServiceDeTeste _financeiro;
    private readonly TurmaRepositorioDeTeste _turmas = new();
    private readonly ProfessorRepositorioDeTeste _professores = new();
    private readonly CoordenadorRepositorioDeTeste _coordenadores = new();
    private readonly EmailServiceDeTeste _emails = new();
    private readonly WhatsAppServiceDeTeste _whatsApp = new();
    private readonly ConfiguracoesCobrancaAutomatica _configuracoes = new();
    private readonly RotinaFinanceiraService _servico;

    public RotinaFinanceiraServiceTest()
    {
        _financeiro = new FinanceiroServiceDeTeste(_parcelas, _avisos);
        _servico = new RotinaFinanceiraService(
            _parcelas, _alunos, _avisos, _financeiro, _turmas, _professores, _coordenadores,
            _emails, _whatsApp, new RelogioFixo(Hoje), _configuracoes);
    }

    private Aluno CriarAluno(string nome = "Joao Silva")
    {
        var professor = new Professor("Prof Carlos", "(41) 99999-1111", Guid.NewGuid());
        _professores.Professores.Add(professor);
        var turma = new Turma("Sub-11 Manha", "Seg,Qua", "09:00", Guid.NewGuid(), professor.Id);
        _turmas.Turmas.Add(turma);
        var aluno = new Aluno(nome, new DateOnly(2015, 3, 1), NivelTecnico.Iniciante, null, "Maria Responsavel",
            "52998224725", "(41) 98888-7777", "Rua A", turma.Id, Guid.NewGuid());
        _alunos.Alunos.Add(aluno);
        return aluno;
    }

    private Parcela CriarParcela(Aluno aluno, int diasAteVencer, int numero = 1)
    {
        var parcela = new Parcela(Guid.NewGuid(), numero, Hoje.AddDays(diasAteVencer), 150m);
        _parcelas.Itens.Add(new ParcelaDetalhada(parcela, aluno.Id, aluno.NomeCompleto, aluno.ResponsavelNome,
            aluno.Telefone, StatusContrato.Ativo));
        return parcela;
    }

    // ---------- Sincronização de status ----------

    [Fact]
    public async Task Aluno_Com_Parcela_Vencida_Vira_Inadimplente()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, -3);

        var (marcados, regularizados) = await _servico.SincronizarStatusDosAlunosAsync();

        Assert.Equal(1, marcados);
        Assert.Equal(0, regularizados);
        Assert.Equal(StatusAluno.Inadimplente, aluno.Status);
    }

    [Fact]
    public async Task Aluno_Sem_Parcela_Vencida_Continua_Ativo()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, 5);

        await _servico.SincronizarStatusDosAlunosAsync();

        Assert.Equal(StatusAluno.Ativo, aluno.Status);
    }

    [Fact]
    public async Task Parcela_Que_Vence_Hoje_Ainda_Nao_E_Atraso()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, 0);

        await _servico.SincronizarStatusDosAlunosAsync();

        Assert.Equal(StatusAluno.Ativo, aluno.Status);
    }

    [Fact]
    public async Task Aluno_Que_Quitou_Volta_A_Ativo()
    {
        var aluno = CriarAluno();
        var parcela = CriarParcela(aluno, -3);
        await _servico.SincronizarStatusDosAlunosAsync();
        Assert.Equal(StatusAluno.Inadimplente, aluno.Status);

        parcela.MarcarComoPaga(Hoje);
        var (_, regularizados) = await _servico.SincronizarStatusDosAlunosAsync();

        Assert.Equal(1, regularizados);
        Assert.Equal(StatusAluno.Ativo, aluno.Status);
    }

    [Fact]
    public async Task Aluno_Inativo_Nao_Muda_De_Status()
    {
        var aluno = CriarAluno();
        aluno.Desativar();
        CriarParcela(aluno, -30);

        // O repositório de teste devolve inativos para provar que o próprio domínio protege o status.
        _alunos.DevolverInativos = true;
        await _servico.SincronizarStatusDosAlunosAsync();

        Assert.Equal(StatusAluno.Inativo, aluno.Status);
    }

    // ---------- Aviso prévio ----------

    [Fact]
    public async Task Parcela_Dentro_Da_Janela_De_10_Dias_Recebe_Aviso_Previo()
    {
        var aluno = CriarAluno();
        var parcela = CriarParcela(aluno, 10);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AvisosPreviosEnviados);
        Assert.Equal(parcela.Id, Assert.Single(_financeiro.ParcelasCobradas));
    }

    [Fact]
    public async Task Parcela_Fora_Da_Janela_Nao_Recebe_Aviso_Previo()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, 11);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AvisosPreviosEnviados);
        Assert.Empty(_financeiro.ParcelasCobradas);
    }

    [Fact]
    public async Task Aviso_Previo_Nao_Se_Repete_Em_Nova_Execucao()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, 7);

        await _servico.ExecutarAsync();
        var segunda = await _servico.ExecutarAsync();

        Assert.Equal(0, segunda.AvisosPreviosEnviados);
        Assert.Equal(1, _financeiro.ParcelasCobradas.Count);
    }

    [Fact]
    public async Task Aluno_Com_Duas_Parcelas_Na_Janela_Recebe_So_Uma_Mensagem()
    {
        var aluno = CriarAluno();
        var maisProxima = CriarParcela(aluno, 3, numero: 1);
        CriarParcela(aluno, 9, numero: 2);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AvisosPreviosEnviados);
        Assert.Equal(maisProxima.Id, Assert.Single(_financeiro.ParcelasCobradas));
    }

    // ---------- Aviso de atraso + equipe ----------

    [Fact]
    public async Task Parcela_Vencida_Ha_10_Dias_Reavisa_O_Responsavel()
    {
        var aluno = CriarAluno();
        var parcela = CriarParcela(aluno, -10);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AvisosDeAtrasoEnviados);
        Assert.Equal(parcela.Id, Assert.Single(_financeiro.ParcelasCobradas));
    }

    [Fact]
    public async Task Parcela_Vencida_Ha_Menos_De_10_Dias_Nao_Reavisa()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, -9);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AvisosDeAtrasoEnviados);
        Assert.Empty(_financeiro.ParcelasCobradas);
    }

    [Fact]
    public async Task Reaviso_De_Atraso_Notifica_Professor_E_Coordenacao()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, -12);
        AdicionarCoordenador("coordenadora@escola.com");

        await _servico.ExecutarAsync();

        var mensagemDoProfessor = Assert.Single(_whatsApp.Mensagens);
        Assert.Contains("Joao Silva", mensagemDoProfessor.Mensagem);
        Assert.Equal("5541999991111", mensagemDoProfessor.Numero);

        var email = Assert.Single(_emails.Enviados);
        Assert.Equal("coordenadora@escola.com", email.Destinatario);
        Assert.Contains("Joao Silva", email.Assunto);
    }

    [Fact]
    public async Task Mensagem_Ao_Professor_Nao_Traz_Valores_Financeiros()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, -12);

        await _servico.ExecutarAsync();

        var mensagem = Assert.Single(_whatsApp.Mensagens).Mensagem;
        Assert.False(mensagem.Contains("150"), "O professor não pode receber valores.");
        Assert.False(mensagem.Contains("R$"), "O professor não pode receber valores.");
    }

    [Fact]
    public async Task Reaviso_De_Atraso_Nao_Se_Repete_Nem_Notifica_De_Novo()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, -15);
        AdicionarCoordenador("coordenadora@escola.com");

        await _servico.ExecutarAsync();
        var segunda = await _servico.ExecutarAsync();

        Assert.Equal(0, segunda.AvisosDeAtrasoEnviados);
        Assert.Equal(1, _financeiro.ParcelasCobradas.Count);
        Assert.Equal(1, _whatsApp.Mensagens.Count);
        Assert.Equal(1, _emails.Enviados.Count);
    }

    [Fact]
    public async Task Aviso_Manual_Antes_Dos_10_Dias_Nao_Impede_O_Reaviso_Automatico()
    {
        var aluno = CriarAluno();
        var parcela = CriarParcela(aluno, -12);
        // Aviso manual enviado quando a parcela tinha 3 dias de atraso (antes do reaviso).
        _avisos.Avisos.Add(AvisoEmDia(parcela.Id, TipoAvisoCobranca.AvisoAtraso, Hoje.AddDays(-9)));

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AvisosDeAtrasoEnviados);
    }

    [Fact]
    public async Task Aluno_Com_Varias_Parcelas_Vencidas_Recebe_So_O_Reaviso_Da_Mais_Antiga()
    {
        var aluno = CriarAluno();
        var maisAntiga = CriarParcela(aluno, -40, numero: 1);
        CriarParcela(aluno, -20, numero: 2);
        CriarParcela(aluno, -11, numero: 3);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AvisosDeAtrasoEnviados);
        Assert.Equal(maisAntiga.Id, Assert.Single(_financeiro.ParcelasCobradas));
    }

    [Fact]
    public async Task Falha_No_Envio_Ao_Responsavel_Conta_Falha_E_Nao_Notifica_A_Equipe()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, -12);
        _financeiro.SimularFalha = true;

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.FalhasDeEnvio);
        Assert.Equal(0, resultado.AvisosDeAtrasoEnviados);
        Assert.Empty(_whatsApp.Mensagens);
        Assert.Empty(_emails.Enviados);
    }

    [Fact]
    public async Task Falha_No_Email_Da_Coordenacao_Nao_Quebra_A_Rotina()
    {
        var aluno = CriarAluno();
        CriarParcela(aluno, -12);
        AdicionarCoordenador("a@escola.com");
        AdicionarCoordenador("b@escola.com");
        _emails.FalharNoPrimeiro = true;

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AvisosDeAtrasoEnviados);
        Assert.Equal(1, _emails.Enviados.Count);
    }

    [Fact]
    public async Task Rotina_Desabilitada_So_Sincroniza_Status_E_Nao_Envia_Nada()
    {
        _configuracoes.Habilitada = false;
        var aluno = CriarAluno();
        CriarParcela(aluno, -20);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(1, resultado.AlunosMarcadosComoInadimplentes);
        Assert.Empty(_financeiro.ParcelasCobradas);
        Assert.Empty(_whatsApp.Mensagens);
    }

    [Fact]
    public async Task Dias_Configuraveis_Sao_Respeitados()
    {
        _configuracoes.DiasAntesDoVencimento = 3;
        _configuracoes.DiasAposVencimentoParaReavisar = 5;
        var alunoPrevio = CriarAluno("Aluno Previo");
        CriarParcela(alunoPrevio, 4);
        var alunoAtraso = CriarAluno("Aluno Atraso");
        CriarParcela(alunoAtraso, -5);

        var resultado = await _servico.ExecutarAsync();

        Assert.Equal(0, resultado.AvisosPreviosEnviados);
        Assert.Equal(1, resultado.AvisosDeAtrasoEnviados);
    }

    // ---------- Aluno: transições de status ----------

    [Fact]
    public void Aluno_Ativo_Pode_Ser_Marcado_Como_Inadimplente_E_Regularizado()
    {
        var aluno = CriarAluno();

        Assert.True(aluno.MarcarComoInadimplente());
        Assert.Equal(StatusAluno.Inadimplente, aluno.Status);
        Assert.False(aluno.MarcarComoInadimplente());
        Assert.True(aluno.RegularizarSituacao());
        Assert.Equal(StatusAluno.Ativo, aluno.Status);
        Assert.False(aluno.RegularizarSituacao());
    }

    [Fact]
    public void Aluno_Inativo_Nao_Vira_Inadimplente_Nem_Ativo()
    {
        var aluno = CriarAluno();
        aluno.Desativar();

        Assert.False(aluno.MarcarComoInadimplente());
        Assert.False(aluno.RegularizarSituacao());
        Assert.Equal(StatusAluno.Inativo, aluno.Status);
    }

    // ---------- Apoio ----------

    private void AdicionarCoordenador(string email)
    {
        var usuario = new Usuario("coord" + _coordenadores.Coordenadores.Count, email, "hash", PerfilUsuario.Coordenador);
        var coordenador = new Coordenador("Coordenadora", usuario.Id);
        typeof(Coordenador).GetProperty(nameof(Coordenador.Usuario))!.SetValue(coordenador, usuario);
        _coordenadores.Coordenadores.Add(coordenador);
    }

    private static AvisoCobranca AvisoEmDia(Guid parcelaId, TipoAvisoCobranca tipo, DateOnly dia)
    {
        var aviso = new AvisoCobranca(parcelaId, tipo, "5541988887777");
        // Meio-dia no Brasil = 15h UTC, longe da virada do dia.
        var dataEnvio = dia.ToDateTime(new TimeOnly(15, 0), DateTimeKind.Utc);
        typeof(AvisoCobranca).GetProperty(nameof(AvisoCobranca.DataEnvio))!.SetValue(aviso, dataEnvio);
        return aviso;
    }

    private sealed class RelogioFixo : IRelogio
    {
        private readonly DateOnly _hoje;
        public RelogioFixo(DateOnly hoje) { _hoje = hoje; }
        public DateOnly HojeNoBrasil() => _hoje;
        public DateTime AgoraNoBrasil() => _hoje.ToDateTime(new TimeOnly(12, 0));
    }

    private sealed class ParcelaRepositorioDeTeste : IParcelaRepository
    {
        public List<ParcelaDetalhada> Itens { get; } = new();

        public Task<IEnumerable<ParcelaDetalhada>> ObterDetalhadasAsync(Guid? alunoId, string? status, int? mes, DateOnly hoje)
        {
            IEnumerable<ParcelaDetalhada> consulta = Itens.Where(item => item.StatusContrato == StatusContrato.Ativo);
            switch (status?.ToLowerInvariant())
            {
                case "pago": consulta = consulta.Where(item => item.Parcela.Status == StatusParcela.Pago); break;
                case "atrasado": consulta = consulta.Where(item => item.Parcela.Status != StatusParcela.Pago && item.Parcela.DataVencimento < hoje); break;
                case "pendente": consulta = consulta.Where(item => item.Parcela.Status != StatusParcela.Pago && item.Parcela.DataVencimento >= hoje); break;
            }
            return Task.FromResult<IEnumerable<ParcelaDetalhada>>(consulta.ToList());
        }

        public Task<ParcelaDetalhada?> ObterDetalhadaPorIdAsync(Guid id) => Task.FromResult(Itens.FirstOrDefault(item => item.Parcela.Id == id));
        public Task AdicionarVariasAsync(IEnumerable<Parcela> parcelas) => throw new NotImplementedException();
        public Task<IEnumerable<Parcela>> ObterComFiltrosAsync(Guid? alunoId, string? status, int? mes) => throw new NotImplementedException();
        public Task<Parcela?> ObterPorIdAsync(Guid id) => Task.FromResult(Itens.FirstOrDefault(item => item.Parcela.Id == id)?.Parcela);
        public Task AtualizarAsync(Parcela parcela) => Task.CompletedTask;
        public Task<IEnumerable<Parcela>> ObterTodasAsync() => Task.FromResult<IEnumerable<Parcela>>(Itens.Select(item => item.Parcela).ToList());
    }

    private sealed class AlunoRepositorioDeTeste : IAlunoRepository
    {
        public List<Aluno> Alunos { get; } = new();
        public bool DevolverInativos { get; set; }

        public Task<IEnumerable<Aluno>> ObterTodosAsync() =>
            Task.FromResult<IEnumerable<Aluno>>(Alunos.Where(aluno => DevolverInativos || aluno.Status != StatusAluno.Inativo).ToList());
        public Task<Aluno?> ObterPorIdAsync(Guid id) => Task.FromResult(Alunos.FirstOrDefault(aluno => aluno.Id == id));
        public Task<Dictionary<Guid, string>> ObterNomesPorIdsAsync(IEnumerable<Guid> alunoIds) => throw new NotImplementedException();
        public Task<List<Aluno>> ObterPorIdsAsync(IEnumerable<Guid> alunoIds) => throw new NotImplementedException();
        public Task<bool> ExisteComMesmoNomeEDataNascimentoAsync(string nome, DateOnly dataNascimento, Guid? ignorarId = null) => throw new NotImplementedException();
        public Task AdicionarAsync(Aluno aluno) => throw new NotImplementedException();
        public Task AtualizarAsync(Aluno aluno) => Task.CompletedTask;
    }

    private sealed class AvisoRepositorioDeTeste : IAvisoCobrancaRepository
    {
        public List<AvisoCobranca> Avisos { get; } = new();
        public Task AdicionarAsync(AvisoCobranca avisoCobranca) { Avisos.Add(avisoCobranca); return Task.CompletedTask; }
        public Task<IEnumerable<AvisoCobranca>> ObterPorParcelaAsync(Guid parcelaId) =>
            Task.FromResult<IEnumerable<AvisoCobranca>>(Avisos.Where(aviso => aviso.ParcelaId == parcelaId).ToList());
    }

    /// <summary>Imita o envio real: registra o aviso na parcela com o tipo certo, como o FinanceiroService faz.</summary>
    private sealed class FinanceiroServiceDeTeste : IFinanceiroService
    {
        private readonly ParcelaRepositorioDeTeste _parcelas;
        private readonly AvisoRepositorioDeTeste _avisos;
        public List<Guid> ParcelasCobradas { get; } = new();
        public bool SimularFalha { get; set; }

        public FinanceiroServiceDeTeste(ParcelaRepositorioDeTeste parcelas, AvisoRepositorioDeTeste avisos)
        {
            _parcelas = parcelas;
            _avisos = avisos;
        }

        public async Task<ParcelaResponseDTO> DispararCobrancaAsync(Guid parcelaId)
        {
            if (SimularFalha) return new ParcelaResponseDTO { Status = false, Message = "Falha ao enviar a cobrança via WhatsApp." };

            var item = _parcelas.Itens.First(candidato => candidato.Parcela.Id == parcelaId);
            ParcelasCobradas.Add(parcelaId);
            var atrasada = item.Parcela.DataVencimento < Hoje;
            var tipo = atrasada ? TipoAvisoCobranca.AvisoAtraso : TipoAvisoCobranca.AvisoPrevio;
            await _avisos.AdicionarAsync(AvisoEmDia(parcelaId, tipo, Hoje));
            return new ParcelaResponseDTO { Status = true };
        }

        public Task<IEnumerable<object>> ListarParcelasAsync(Guid? alunoId, string? status, int? mes) => throw new NotImplementedException();
        public Task<ParcelaResponseDTO> RegistrarPagamentoAsync(Guid parcelaId) => throw new NotImplementedException();
        public Task<ResumoFinanceiroResponseDTO> ObterResumoAsync() => throw new NotImplementedException();
    }

    private sealed class TurmaRepositorioDeTeste : ITurmaRepository
    {
        public List<Turma> Turmas { get; } = new();
        public Task<Turma> ObterPorIdAsync(Guid id) => Task.FromResult(Turmas.First(turma => turma.Id == id));
        public Task<IEnumerable<Turma>> ObterTodasAsync() => Task.FromResult<IEnumerable<Turma>>(Turmas.ToList());
        public Task<IEnumerable<Turma>> ObterPorCategoriaAsync(Guid categoriaId) => throw new NotImplementedException();
        public Task<int> ContarAlunosPorTurmaAsync(Guid turmaId) => throw new NotImplementedException();
        public Task AdicionarAsync(Turma turma) => throw new NotImplementedException();
        public Task AtualizarAsync(Turma turma) => throw new NotImplementedException();
        public Task RemoverAsync(Turma turma) => throw new NotImplementedException();
        public Task<bool> PossuiVinculosAsync(Guid turmaId) => throw new NotImplementedException();
    }

    private sealed class ProfessorRepositorioDeTeste : IProfessorRepository
    {
        public List<Professor> Professores { get; } = new();
        public Task<IEnumerable<Professor>> ObterTodosAsync() => Task.FromResult<IEnumerable<Professor>>(Professores.ToList());
        public Task<Professor?> ObterPorIdAsync(Guid id) => Task.FromResult(Professores.FirstOrDefault(professor => professor.Id == id));
        public Task AdicionarAsync(Professor professor) => throw new NotImplementedException();
        public Task AtualizarAsync(Professor professor) => throw new NotImplementedException();
        public Task RemoverAsync(Professor professor) => throw new NotImplementedException();
        public Task<bool> PossuiTurmasAsync(Guid professorId) => throw new NotImplementedException();
    }

    private sealed class CoordenadorRepositorioDeTeste : ICoordenadorRepository
    {
        public List<Coordenador> Coordenadores { get; } = new();
        public Task<IEnumerable<Coordenador>> ObterTodosAsync() => Task.FromResult<IEnumerable<Coordenador>>(Coordenadores.ToList());
        public Task<Coordenador?> ObterPorIdAsync(Guid id) => Task.FromResult(Coordenadores.FirstOrDefault(coordenador => coordenador.Id == id));
        public Task AdicionarAsync(Coordenador coordenador) => throw new NotImplementedException();
        public Task AtualizarAsync(Coordenador coordenador) => throw new NotImplementedException();
        public Task RemoverAsync(Coordenador coordenador) => throw new NotImplementedException();
        public Task<bool> EmailJaCadastradoAsync(string email, Guid? ignorarUsuarioId = null) => throw new NotImplementedException();
    }

    private sealed class EmailServiceDeTeste : IEmailService
    {
        public List<(string Destinatario, string Assunto, string Corpo)> Enviados { get; } = new();
        public bool FalharNoPrimeiro { get; set; }
        private bool _primeiroJaFalhou;

        public Task EnviarAsync(string destinatario, string assunto, string corpoTexto)
        {
            if (FalharNoPrimeiro && !_primeiroJaFalhou)
            {
                _primeiroJaFalhou = true;
                throw new InvalidOperationException("SMTP indisponível");
            }
            Enviados.Add((destinatario, assunto, corpoTexto));
            return Task.CompletedTask;
        }
    }

    private sealed class WhatsAppServiceDeTeste : IWhatsAppService
    {
        public List<(string Numero, string Mensagem)> Mensagens { get; } = new();
        public Task<bool> EnviarMensagemAsync(string numeroTelefone, string mensagem)
        {
            Mensagens.Add((numeroTelefone, mensagem));
            return Task.FromResult(true);
        }
    }
}
