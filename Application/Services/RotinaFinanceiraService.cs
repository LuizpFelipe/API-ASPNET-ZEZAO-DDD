using System.Globalization;
using Application.Configuration;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class RotinaFinanceiraService : IRotinaFinanceiraService
{
    private const int DiferencaDoFusoDoBrasilEmHoras = -3;

    private readonly IParcelaRepository _parcelaRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly IAvisoCobrancaRepository _avisoCobrancaRepository;
    private readonly IFinanceiroService _financeiroService;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IProfessorRepository _professorRepository;
    private readonly ICoordenadorRepository _coordenadorRepository;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IRelogio _relogio;
    private readonly ConfiguracoesCobrancaAutomatica _configuracoes;

    public RotinaFinanceiraService(
        IParcelaRepository parcelaRepository,
        IAlunoRepository alunoRepository,
        IAvisoCobrancaRepository avisoCobrancaRepository,
        IFinanceiroService financeiroService,
        ITurmaRepository turmaRepository,
        IProfessorRepository professorRepository,
        ICoordenadorRepository coordenadorRepository,
        IEmailService emailService,
        IWhatsAppService whatsAppService,
        IRelogio relogio,
        ConfiguracoesCobrancaAutomatica configuracoes)
    {
        _parcelaRepository = parcelaRepository;
        _alunoRepository = alunoRepository;
        _avisoCobrancaRepository = avisoCobrancaRepository;
        _financeiroService = financeiroService;
        _turmaRepository = turmaRepository;
        _professorRepository = professorRepository;
        _coordenadorRepository = coordenadorRepository;
        _emailService = emailService;
        _whatsAppService = whatsAppService;
        _relogio = relogio;
        _configuracoes = configuracoes;
    }

    public async Task<(int Marcados, int Regularizados)> SincronizarStatusDosAlunosAsync()
    {
        var hoje = _relogio.HojeNoBrasil();
        var parcelasAtrasadas = await _parcelaRepository.ObterDetalhadasAsync(null, "atrasado", null, hoje);
        var alunosComAtraso = parcelasAtrasadas.Select(parcela => parcela.AlunoId).ToHashSet();

        var marcados = 0;
        var regularizados = 0;

        foreach (var aluno in await _alunoRepository.ObterTodosAsync())
        {
            var temParcelaVencida = alunosComAtraso.Contains(aluno.Id);

            if (temParcelaVencida && aluno.MarcarComoInadimplente())
            {
                await _alunoRepository.AtualizarAsync(aluno);
                marcados++;
            }
            else if (!temParcelaVencida && aluno.RegularizarSituacao())
            {
                await _alunoRepository.AtualizarAsync(aluno);
                regularizados++;
            }
        }

        return (marcados, regularizados);
    }

    public async Task<ResultadoDaRotinaFinanceiraDTO> ExecutarAsync()
    {
        var resultado = new ResultadoDaRotinaFinanceiraDTO();

        (resultado.AlunosMarcadosComoInadimplentes, resultado.AlunosRegularizados) = await SincronizarStatusDosAlunosAsync();

        if (!_configuracoes.Habilitada)
            return resultado;

        var hoje = _relogio.HojeNoBrasil();

        await EnviarAvisosPreviosAsync(hoje, resultado);
        await EnviarAvisosDeAtrasoAsync(hoje, resultado);

        return resultado;
    }

    private async Task EnviarAvisosPreviosAsync(DateOnly hoje, ResultadoDaRotinaFinanceiraDTO resultado)
    {
        var limiteDoAviso = hoje.AddDays(_configuracoes.DiasAntesDoVencimento);
        var parcelasPendentes = await _parcelaRepository.ObterDetalhadasAsync(null, "pendente", null, hoje);

        // Uma mensagem por aluno: só a parcela que vence primeiro.
        var proximaParcelaDeCadaAluno = parcelasPendentes
            .Where(item => item.StatusContrato == StatusContrato.Ativo && item.Parcela.DataVencimento <= limiteDoAviso)
            .GroupBy(item => item.AlunoId)
            .Select(grupo => grupo.OrderBy(item => item.Parcela.DataVencimento).First());

        foreach (var item in proximaParcelaDeCadaAluno)
        {
            var avisosAnteriores = await _avisoCobrancaRepository.ObterPorParcelaAsync(item.Parcela.Id);
            if (avisosAnteriores.Any(aviso => aviso.Tipo == TipoAvisoCobranca.AvisoPrevio))
                continue;

            var resposta = await _financeiroService.DispararCobrancaAsync(item.Parcela.Id);
            if (resposta.Status) resultado.AvisosPreviosEnviados++;
            else resultado.FalhasDeEnvio++;
        }
    }

    private async Task EnviarAvisosDeAtrasoAsync(DateOnly hoje, ResultadoDaRotinaFinanceiraDTO resultado)
    {
        var diasParaReavisar = _configuracoes.DiasAposVencimentoParaReavisar;
        var parcelasAtrasadas = await _parcelaRepository.ObterDetalhadasAsync(null, "atrasado", null, hoje);

        // Uma mensagem por aluno: só a parcela vencida mais antiga, para o responsável não ser inundado de avisos.
        var parcelaMaisAntigaDeCadaAluno = parcelasAtrasadas
            .Where(item => item.StatusContrato == StatusContrato.Ativo
                           && item.Parcela.DataVencimento.AddDays(diasParaReavisar) <= hoje)
            .GroupBy(item => item.AlunoId)
            .Select(grupo => grupo.OrderBy(item => item.Parcela.DataVencimento).First());

        foreach (var item in parcelaMaisAntigaDeCadaAluno)
        {
            var dataDoReaviso = item.Parcela.DataVencimento.AddDays(diasParaReavisar);
            var avisosAnteriores = await _avisoCobrancaRepository.ObterPorParcelaAsync(item.Parcela.Id);

            // Um aviso manual enviado antes dos 10 dias não conta: o reaviso é o de depois do prazo.
            var jaReavisado = avisosAnteriores.Any(aviso =>
                aviso.Tipo == TipoAvisoCobranca.AvisoAtraso
                && DataDoEnvioNoBrasil(aviso) >= dataDoReaviso);
            if (jaReavisado)
                continue;

            var resposta = await _financeiroService.DispararCobrancaAsync(item.Parcela.Id);
            if (!resposta.Status)
            {
                resultado.FalhasDeEnvio++;
                continue;
            }

            resultado.AvisosDeAtrasoEnviados++;
            await NotificarEquipeAsync(item, diasParaReavisar);
        }
    }

    private static DateOnly DataDoEnvioNoBrasil(AvisoCobranca aviso)
    {
        return DateOnly.FromDateTime(aviso.DataEnvio.AddHours(DiferencaDoFusoDoBrasilEmHoras));
    }

    private async Task NotificarEquipeAsync(ParcelaDetalhada item, int diasDeAtraso)
    {
        await NotificarProfessorAsync(item, diasDeAtraso);
        await NotificarCoordenacaoAsync(item, diasDeAtraso);
    }

    private async Task NotificarProfessorAsync(ParcelaDetalhada item, int diasDeAtraso)
    {
        try
        {
            var aluno = await _alunoRepository.ObterPorIdAsync(item.AlunoId);
            if (aluno == null) return;

            var turma = await _turmaRepository.ObterPorIdAsync(aluno.TurmaId);
            if (turma == null) return;

            var professor = await _professorRepository.ObterPorIdAsync(turma.ProfessorId);
            var telefone = NormalizarTelefone(professor?.Telefone);
            if (telefone == null) return;

            // O professor nunca vê dados financeiros: a mensagem não traz valores nem parcelas.
            var mensagem = $"Aviso da secretaria: o aluno {item.AlunoNome} (turma {turma.Nome}) está com pendência há mais de {diasDeAtraso} dias. Siga a orientação da coordenação para a entrada em quadra. ZezaoFutsal";
            await _whatsAppService.EnviarMensagemAsync(telefone, mensagem);
        }
        catch (Exception)
        {
            // A notificação à equipe é complementar: falhar aqui não pode desfazer a cobrança já enviada.
        }
    }

    private async Task NotificarCoordenacaoAsync(ParcelaDetalhada item, int diasDeAtraso)
    {
        try
        {
            var valor = item.Parcela.Valor.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
            var vencimento = item.Parcela.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var assunto = $"Inadimplência: {item.AlunoNome} está há mais de {diasDeAtraso} dias sem pagar";
            var corpo = $"O aluno {item.AlunoNome} (responsável: {item.ResponsavelNome}) está com a parcela {item.Parcela.Numero}, " +
                        $"de R$ {valor}, vencida em {vencimento} e sem pagamento. O responsável já foi avisado novamente pelo sistema.";

            foreach (var coordenador in await _coordenadorRepository.ObterTodosAsync())
            {
                var email = coordenador.Usuario?.Email;
                if (string.IsNullOrWhiteSpace(email)) continue;

                try
                {
                    await _emailService.EnviarAsync(email, assunto, corpo);
                }
                catch (Exception)
                {
                    // Um e-mail com problema não pode impedir os demais.
                }
            }
        }
        catch (Exception)
        {
            // Mesma regra: notificação complementar.
        }
    }

    private static string? NormalizarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return null;

        var digitos = new string(telefone.Where(char.IsDigit).ToArray());

        if (digitos.StartsWith("55") && (digitos.Length == 12 || digitos.Length == 13)) return digitos;
        if (digitos.Length == 10 || digitos.Length == 11) return "55" + digitos;

        return null;
    }
}
