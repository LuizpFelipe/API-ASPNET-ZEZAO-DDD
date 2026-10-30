using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class PromocaoDeCategoriaService : IPromocaoDeCategoriaService
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IRelogio _relogio;

    public PromocaoDeCategoriaService(
        IAlunoRepository alunoRepository,
        ICategoriaRepository categoriaRepository,
        ITurmaRepository turmaRepository,
        IWhatsAppService whatsAppService,
        IRelogio relogio)
    {
        _alunoRepository = alunoRepository;
        _categoriaRepository = categoriaRepository;
        _turmaRepository = turmaRepository;
        _whatsAppService = whatsAppService;
        _relogio = relogio;
    }

    public async Task<ResultadoDaPromocaoDeCategoriaDTO> ExecutarAsync()
    {
        var resultado = new ResultadoDaPromocaoDeCategoriaDTO();
        var hoje = _relogio.HojeNoBrasil();
        var categorias = (await _categoriaRepository.ObterTodasAsync()).ToList();
        var categoriasPorId = categorias.ToDictionary(categoria => categoria.Id);

        foreach (var aluno in await _alunoRepository.ObterTodosAsync())
        {
            if (aluno.Status == StatusAluno.Inativo) continue;
            if (!categoriasPorId.TryGetValue(aluno.CategoriaId, out var categoriaAtual)) continue;

            var idade = CalcularIdade(aluno.DataNascimento, hoje);
            var categoriaDaIdade = categorias.FirstOrDefault(categoria => idade >= categoria.IdadeMin && idade <= categoria.IdadeMax);

            // Só sobe: se a idade não cai em nenhuma categoria, ou cai em uma igual ou abaixo da atual, não mexe.
            if (categoriaDaIdade == null || categoriaDaIdade.IdadeMin <= categoriaAtual.IdadeMin) continue;

            var turmaAtual = await _turmaRepository.ObterPorIdAsync(aluno.TurmaId);
            var turmaNova = await EscolherTurmaAsync(categoriaDaIdade.Id, turmaAtual);

            if (turmaNova == null)
            {
                resultado.AlunosSemTurmaNaNovaCategoria.Add($"{aluno.NomeCompleto} (para {categoriaDaIdade.Nome})");
                continue;
            }

            aluno.PromoverParaCategoria(categoriaDaIdade.Id, turmaNova.Id);
            await _alunoRepository.AtualizarAsync(aluno);
            resultado.AlunosPromovidos++;

            await AvisarResponsavelAsync(aluno, categoriaDaIdade, turmaNova);
        }

        return resultado;
    }

    /// <summary>Prefere a turma do mesmo professor e horário, depois a do mesmo professor, depois a do mesmo horário.</summary>
    private async Task<Turma?> EscolherTurmaAsync(Guid categoriaId, Turma? turmaAtual)
    {
        var candidatas = (await _turmaRepository.ObterPorCategoriaAsync(categoriaId)).ToList();
        if (candidatas.Count == 0) return null;

        return candidatas
            .OrderByDescending(turma => turmaAtual != null && turma.ProfessorId == turmaAtual.ProfessorId && turma.Horario == turmaAtual.Horario)
            .ThenByDescending(turma => turmaAtual != null && turma.ProfessorId == turmaAtual.ProfessorId)
            .ThenByDescending(turma => turmaAtual != null && turma.Horario == turmaAtual.Horario)
            .ThenBy(turma => turma.Nome)
            .First();
    }

    private async Task AvisarResponsavelAsync(Aluno aluno, Categoria categoriaNova, Turma turmaNova)
    {
        try
        {
            var telefone = NormalizarTelefone(aluno.Telefone);
            if (telefone == null) return;

            var mensagem = $"Olá, {aluno.ResponsavelNome}! O aluno {aluno.NomeCompleto} completou a idade da categoria {categoriaNova.Nome} " +
                           $"e passou para a turma {turmaNova.Nome} ({turmaNova.DiasSemana}, {turmaNova.Horario}). ZezaoFutsal";
            await _whatsAppService.EnviarMensagemAsync(telefone, mensagem);
        }
        catch (Exception)
        {
            // O aviso é complementar: a promoção já foi salva.
        }
    }

    private static int CalcularIdade(DateOnly dataNascimento, DateOnly hoje)
    {
        var idade = hoje.Year - dataNascimento.Year;
        if (dataNascimento > hoje.AddYears(-idade)) idade--;
        return idade;
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
