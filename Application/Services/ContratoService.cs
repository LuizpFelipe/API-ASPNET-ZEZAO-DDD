using Application.Configuration;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class ContratoService : IContratoService
{
    private readonly IContratoRepository _contratoRepository;
    private readonly IParcelaRepository _parcelaRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IContratoPdfService _pdfService;
    private readonly ConfiguracoesContrato _configuracoes;

    public ContratoService(IContratoRepository contratoRepository, IParcelaRepository parcelaRepository,
                           IAlunoRepository alunoRepository, ITurmaRepository turmaRepository,
                           IContratoPdfService pdfService, ConfiguracoesContrato configuracoes)
    {
        _contratoRepository = contratoRepository;
        _parcelaRepository = parcelaRepository;
        _alunoRepository = alunoRepository;
        _turmaRepository = turmaRepository;
        _pdfService = pdfService;
        _configuracoes = configuracoes;
    }

    public async Task<ContratoResponseDTO> CriarAsync(Guid alunoId)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(alunoId);
        if (aluno == null)
            return new ContratoResponseDTO { Status = false, Message = "Aluno não encontrado." };

        if (aluno.Status == StatusAluno.Inativo)
            return new ContratoResponseDTO { Status = false, Message = "Não é possível criar contrato para um aluno inativo." };

        if (await _contratoRepository.ExisteContratoAtivoDoAlunoAsync(alunoId))
            return new ContratoResponseDTO { Status = false, Message = "O aluno já possui um contrato ativo." };

        var dataInicio = DateOnly.FromDateTime(DateTime.Now);
        var dataFim = dataInicio.AddYears(1);

        var contrato = new Contrato(alunoId, dataInicio, dataFim, _configuracoes.ValorParcela, _configuracoes.QuantidadeParcelas);
        await _contratoRepository.AdicionarAsync(contrato);

        var primeiroVencimento = new DateOnly(dataInicio.Year, dataInicio.Month, _configuracoes.DiaVencimento);
        if (primeiroVencimento <= dataInicio)
        {
            primeiroVencimento = primeiroVencimento.AddMonths(1);
        }

        var parcelas = Enumerable.Range(1, _configuracoes.QuantidadeParcelas)
            .Select(numero => new Parcela(contrato.Id, numero, primeiroVencimento.AddMonths(numero - 1), contrato.ValorParcela))
            .ToList();
        await _parcelaRepository.AdicionarVariasAsync(parcelas);

        return new ContratoResponseDTO
        {
            Status = true,
            Message = "Contrato criado com sucesso.",
            Data = Mapear(contrato)
        };
    }

    public async Task<ContratoResponseDTO> ListarAsync()
    {
        var contratos = await _contratoRepository.ObterTodosAsync();
        return new ContratoResponseDTO { Status = true, Data = contratos.Select(Mapear).ToList() };
    }

    public async Task<ContratoResponseDTO> ObterPorIdAsync(Guid id)
    {
        var contrato = await _contratoRepository.ObterPorIdAsync(id);
        if (contrato == null) return new ContratoResponseDTO { Status = false, Message = "Contrato não encontrado." };

        return new ContratoResponseDTO { Status = true, Data = Mapear(contrato) };
    }

    public async Task<ContratoResponseDTO> AtualizarAsync(Guid id, AtualizarContratoRequestDTO dto)
    {
        var contrato = await _contratoRepository.ObterPorIdAsync(id);
        if (contrato == null) return new ContratoResponseDTO { Status = false, Message = "Contrato não encontrado." };

        if (dto.Status != StatusContrato.Cancelado)
        {
            return new ContratoResponseDTO { Status = false, Message = "Apenas o cancelamento do contrato é suportado." };
        }

        if (contrato.Status == StatusContrato.Cancelado)
        {
            return new ContratoResponseDTO { Status = false, Message = "Contrato já está cancelado." };
        }

        contrato.Cancelar();
        await _contratoRepository.AtualizarAsync(contrato);

        return new ContratoResponseDTO { Status = true, Message = "Contrato cancelado com sucesso.", Data = Mapear(contrato) };
    }

    public async Task<ContratoResponseDTO> GerarPdfAsync(Guid id)
    {
        var contrato = await _contratoRepository.ObterPorIdAsync(id);
        if (contrato == null) return new ContratoResponseDTO { Status = false, Message = "Contrato não encontrado." };

        var aluno = await _alunoRepository.ObterPorIdAsync(contrato.AlunoId);
        var turma = aluno == null ? null : await _turmaRepository.ObterPorIdAsync(aluno.TurmaId);
        if (aluno == null || turma == null)
        {
            return new ContratoResponseDTO { Status = false, Message = "Dados do aluno do contrato não encontrados." };
        }

        if (string.IsNullOrWhiteSpace(aluno.ResponsavelCpf))
        {
            return new ContratoResponseDTO { Status = false, Message = "Informe o CPF do responsável no cadastro do aluno antes de gerar o contrato." };
        }

        var pdf = _pdfService.Gerar(contrato, aluno, aluno.Categoria?.Nome ?? string.Empty, turma.Nome, _configuracoes);

        return new ContratoResponseDTO { Status = true, Message = "PDF gerado com sucesso.", Data = pdf };
    }

    private static ContratoDataDTO Mapear(Contrato contrato) => new ContratoDataDTO
    {
        Id = contrato.Id,
        AlunoId = contrato.AlunoId,
        DataInicio = contrato.DataInicio,
        DataFim = contrato.DataFim,
        ValorParcela = contrato.ValorParcela,
        NumParcelas = contrato.NumParcelas,
        Status = contrato.Status,
        PdfPath = contrato.PdfPath
    };
}
