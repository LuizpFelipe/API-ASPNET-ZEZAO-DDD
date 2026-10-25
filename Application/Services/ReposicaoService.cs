using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services;

public class ReposicaoService : IReposicaoService
{
    private const int HorasMinimasAviso = 24;

    private static readonly Dictionary<string, DayOfWeek> DiasPorAbreviacao = new()
    {
        ["dom"] = DayOfWeek.Sunday,
        ["seg"] = DayOfWeek.Monday,
        ["ter"] = DayOfWeek.Tuesday,
        ["qua"] = DayOfWeek.Wednesday,
        ["qui"] = DayOfWeek.Thursday,
        ["sex"] = DayOfWeek.Friday,
        ["sab"] = DayOfWeek.Saturday
    };

    private readonly IReposicaoRepository _reposicaoRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IRelogio _relogio;

    public ReposicaoService(IReposicaoRepository reposicaoRepository, IAlunoRepository alunoRepository,
                            ITurmaRepository turmaRepository, IRelogio relogio)
    {
        _reposicaoRepository = reposicaoRepository;
        _alunoRepository = alunoRepository;
        _turmaRepository = turmaRepository;
        _relogio = relogio;
    }

    public async Task<ReposicaoResponseDTO> ListarAsync()
    {
        var reposicoes = await _reposicaoRepository.ListarTodasAsync();
        return new ReposicaoResponseDTO { Status = true, Data = reposicoes.Select(Mapear).ToList() };
    }

    public async Task<ReposicaoResponseDTO> RegistrarAsync(RegistrarReposicaoRequestDTO dto)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(dto.AlunoId);
        if (aluno == null || aluno.Status == StatusAluno.Inativo)
            return Falha("Aluno não encontrado.");

        var turmaOrigem = await _turmaRepository.ObterPorIdAsync(dto.TurmaOrigemId);
        if (turmaOrigem == null)
            return Falha("Turma de origem não encontrada.");

        var turmaDestino = await _turmaRepository.ObterPorIdAsync(dto.TurmaDestinoId);
        if (turmaDestino == null)
            return Falha("Turma de destino não encontrada.");

        // O aluno precisa ser da turma de origem e a aula perdida precisa ter existido nessa data
        if (aluno.TurmaId != turmaOrigem.Id)
            return Falha("O aluno não pertence à turma de origem.");

        if (!turmaOrigem.TemAulaEm(dto.DataOriginal))
            return Falha("A turma de origem não tem aula na data informada.");

        // Regra 1: mesma categoria e turma diferente da origem
        if (turmaDestino.Id == turmaOrigem.Id || turmaDestino.CategoriaId != turmaOrigem.CategoriaId)
            return Falha("A turma de destino precisa ser de outra turma da mesma categoria da turma de origem.");

        // Regra 2: mesmo mês/ano da aula perdida
        if (dto.DataReposicao.Year != dto.DataOriginal.Year || dto.DataReposicao.Month != dto.DataOriginal.Month)
            return Falha("A reposição precisa ser no mesmo mês da aula perdida.");

        // Regra 4: a turma de destino precisa ter aula nesse dia da semana
        var diasDaTurma = LerDiasSemana(turmaDestino.DiasSemana);
        if (diasDaTurma.Count == 0)
            return Falha("Não foi possível interpretar os dias da semana da turma de destino.");

        if (!diasDaTurma.Contains(dto.DataReposicao.DayOfWeek))
            return Falha("A turma de destino não tem aula no dia da semana da data de reposição.");

        // Regra 3: aviso mínimo de 24h, no horário do Brasil
        var horario = LerHorario(turmaDestino.Horario);
        if (horario == null)
            return Falha("Não foi possível interpretar o horário da turma de destino.");

        var dataHoraAula = dto.DataReposicao.ToDateTime(horario.Value);
        if (dataHoraAula < _relogio.AgoraNoBrasil().AddHours(HorasMinimasAviso))
            return Falha($"A reposição precisa ser registrada com pelo menos {HorasMinimasAviso}h de antecedência da aula de destino.");

        if (await _reposicaoRepository.ExisteReposicaoNoMesmoDiaAsync(dto.AlunoId, dto.DataOriginal))
            return Falha("Já existe uma reposição registrada para esta aula perdida.");

        var reposicao = new Reposicao(dto.AlunoId, dto.TurmaOrigemId, dto.TurmaDestinoId, dto.DataOriginal, dto.DataReposicao);
        await _reposicaoRepository.AdicionarAsync(reposicao);

        return new ReposicaoResponseDTO
        {
            Status = true,
            Message = "Reposição registrada com sucesso.",
            Data = Mapear(reposicao)
        };
    }

    private static HashSet<DayOfWeek> LerDiasSemana(string? diasSemana)
    {
        var dias = new HashSet<DayOfWeek>();
        foreach (Match trecho in Regex.Matches(diasSemana ?? string.Empty, @"\p{L}+"))
        {
            if (DiasPorAbreviacao.TryGetValue(Normalizar(trecho.Value), out var dia))
                dias.Add(dia);
        }
        return dias;
    }

    private static TimeOnly? LerHorario(string? horario)
    {
        var trecho = Regex.Match(horario ?? string.Empty, @"(\d{1,2})\s*(?:h|:)\s*(\d{2})?", RegexOptions.IgnoreCase);
        if (!trecho.Success) return null;

        var hora = int.Parse(trecho.Groups[1].Value);
        var minuto = trecho.Groups[2].Success ? int.Parse(trecho.Groups[2].Value) : 0;
        if (hora > 23 || minuto > 59) return null;

        return new TimeOnly(hora, minuto);
    }

    private static string Normalizar(string texto)
    {
        var decomposto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var semAcento = new string(decomposto
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        return semAcento.Length > 3 ? semAcento[..3] : semAcento;
    }

    private static ReposicaoResponseDTO Falha(string mensagem) => new ReposicaoResponseDTO { Status = false, Message = mensagem };

    private static ReposicaoDataDTO Mapear(Reposicao reposicao) => new ReposicaoDataDTO
    {
        Id = reposicao.Id,
        AlunoId = reposicao.AlunoId,
        TurmaOrigemId = reposicao.TurmaOrigemId,
        TurmaDestinoId = reposicao.TurmaDestinoId,
        DataOriginal = reposicao.DataOriginal,
        DataReposicao = reposicao.DataReposicao,
        DataRegistro = reposicao.DataRegistro
    };
}