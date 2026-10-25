using Application.Configuration;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using System.Globalization;

namespace Application.Services
{
    public class PresencaService : IPresencaService
    {
        private readonly IPresencaRepository _presencaRepository;
        private readonly IRelogio _relogio;
        private readonly ConfiguracoesPresenca _configuracoes;

        public PresencaService(IPresencaRepository presencaRepository, IRelogio relogio, ConfiguracoesPresenca configuracoes)
        {
            _presencaRepository = presencaRepository;
            _relogio = relogio;
            _configuracoes = configuracoes;
        }

        public async Task<TurmaDoProfessorResponseDTO> ListarTurmasDoProfessorAsync(string nomeUsuario, bool ehCoordenador, DateOnly? data = null, Guid? professorIdFiltro = null)
        {
            Guid? professorId = ehCoordenador ? professorIdFiltro : null;

            if (!ehCoordenador)
            {
                var professor = await _presencaRepository.ObterProfessorPorNomeUsuarioAsync(nomeUsuario);
                if (professor == null)
                {
                    return new TurmaDoProfessorResponseDTO
                    {
                        Status = false,
                        Message = "Nenhum professor vinculado a este usuário.",
                        Falha = TipoFalhaPresenca.NaoEncontrado
                    };
                }

                professorId = professor.Id;
            }

            var turmas = (await _presencaRepository.ObterTurmasAsync(professorId)).ToList();

            if (data.HasValue)
                turmas = turmas.Where(t => t.TemAulaEm(data.Value)).ToList();

            var lista = turmas.Select(t => new TurmaDoProfessorDataDTO
            {
                Id = t.Id,
                Nome = t.Nome,
                DiasSemana = t.DiasSemana,
                Horario = t.Horario,
                CategoriaId = t.CategoriaId,
                ProfessorId = t.ProfessorId,
                TemAulaNaData = data.HasValue ? true : (bool?)null
            }).ToList();

            string mensagem;
            if (lista.Count == 0 && data.HasValue)
                mensagem = "Nenhuma aula neste dia.";
            else if (lista.Count == 0)
                mensagem = ehCoordenador ? "Ainda não há turmas cadastradas." : "Você ainda não tem turmas cadastradas.";
            else
                mensagem = lista.Count == 1 ? "1 turma encontrada." : $"{lista.Count} turmas encontradas.";

            return new TurmaDoProfessorResponseDTO
            {
                Status = true,
                Message = mensagem,
                Data = lista
            };
        }

        public async Task<ChamadaResponseDTO> ObterChamadaAsync(Guid turmaId, DateOnly? data, string nomeUsuario, bool ehCoordenador)
        {
            var (turma, erro) = await ValidarAcessoAsync(turmaId, nomeUsuario, ehCoordenador);
            if (erro != null) return erro;

            if (data == null)
                return Falha("Informe a data da chamada. Exemplo: ?data=2026-09-26", TipoFalhaPresenca.Invalido);

            var alunos = (await _presencaRepository.ObterAlunosDaTurmaAsync(turmaId)).ToList();
            var presencas = await _presencaRepository.ObterPresencasAsync(turmaId, data.Value);

            var (podeEditar, motivo) = AvaliarEdicao(turma!, data.Value, ehCoordenador);
            var chamada = MontarChamada(turma!, data.Value, alunos, presencas, podeEditar, motivo);

            string mensagem;
            if (chamada.Alunos.Count == 0)
                mensagem = "Esta turma ainda não tem alunos ativos.";
            else if (chamada.Alunos.Any(a => a.Presente != null))
                mensagem = "Chamada carregada. Esta data já tem presenças registradas.";
            else
                mensagem = "Chamada carregada. Ainda não há presenças registradas nesta data.";

            return Sucesso(mensagem, chamada);
        }

        public async Task<ChamadaResponseDTO> RegistrarChamadaAsync(Guid turmaId, RegistrarPresencaRequestDTO dto, string nomeUsuario, bool ehCoordenador)
        {
            var (turma, erro) = await ValidarAcessoAsync(turmaId, nomeUsuario, ehCoordenador);
            if (erro != null) return erro;

            if (dto == null || dto.Data == default)
                return Falha("Informe a data da chamada.", TipoFalhaPresenca.Invalido);

            var (podeEditar, motivo) = AvaliarEdicao(turma!, dto.Data, ehCoordenador);
            if (!podeEditar)
                return Falha(motivo!, TipoFalhaPresenca.Invalido);

            if (dto.Presencas == null || dto.Presencas.Count == 0)
                return Falha("Informe ao menos um aluno na lista de presenças.", TipoFalhaPresenca.Invalido);

            if (dto.Presencas.GroupBy(p => p.AlunoId).Any(g => g.Count() > 1))
                return Falha("Há aluno repetido na lista de presenças.", TipoFalhaPresenca.Invalido);

            var alunos = (await _presencaRepository.ObterAlunosDaTurmaAsync(turmaId)).ToList();
            var idsDaTurma = alunos.Select(a => a.Id).ToHashSet();

            if (dto.Presencas.Any(p => !idsDaTurma.Contains(p.AlunoId)))
                return Falha("Há aluno na lista que não pertence a esta turma ou está inativo.", TipoFalhaPresenca.Invalido);

            var existentes = await _presencaRepository.ObterPresencasAsync(turmaId, dto.Data);
            var novas = new List<Presenca>();

            foreach (var item in dto.Presencas)
            {
                var existente = existentes.FirstOrDefault(p => p.AlunoId == item.AlunoId);

                if (existente != null)
                    existente.Marcar(item.Presente);
                else
                    novas.Add(new Presenca(item.AlunoId, turmaId, dto.Data, item.Presente));
            }

            await _presencaRepository.SalvarChamadaAsync(novas);

            existentes.AddRange(novas);

            var presentes = dto.Presencas.Count(p => p.Presente);
            var ausentes = dto.Presencas.Count - presentes;
            var acao = novas.Count == 0 ? "atualizada" : "registrada";
            var mensagem = $"Chamada {acao}: {Plural(presentes, "presente", "presentes")} e {Plural(ausentes, "ausente", "ausentes")}.";

            return Sucesso(mensagem, MontarChamada(turma!, dto.Data, alunos, existentes, true, null));
        }

        public async Task<ChamadaMensalResponseDTO> ObterChamadaMensalAsync(Guid turmaId, string? mes, string nomeUsuario, bool ehCoordenador)
        {
            var (turma, erro) = await ValidarAcessoAsync(turmaId, nomeUsuario, ehCoordenador);
            if (erro != null) return FalhaMensal(erro.Message, erro.Falha);

            if (!DateOnly.TryParseExact($"{mes}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var inicioDoMes))
                return FalhaMensal("Informe o mês no formato aaaa-mm. Exemplo: ?mes=2026-10", TipoFalhaPresenca.Invalido);

            var fimDoMes = inicioDoMes.AddMonths(1).AddDays(-1);

            var datasDeAula = Enumerable.Range(0, fimDoMes.Day)
                .Select(deslocamento => inicioDoMes.AddDays(deslocamento))
                .Where(turma!.TemAulaEm)
                .ToList();

            var alunos = (await _presencaRepository.ObterAlunosDaTurmaAsync(turmaId)).ToList();
            var presencas = await _presencaRepository.ObterPresencasDoMesAsync(turmaId, inicioDoMes, fimDoMes);

            var presentePorAlunoEData = presencas
                .GroupBy(p => (p.AlunoId, p.Data))
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First().Presente);

            var data = new ChamadaMensalDataDTO
            {
                Turma = new ChamadaMensalTurmaDTO
                {
                    Id = turma!.Id,
                    Nome = turma.Nome,
                    DiasSemana = turma.DiasSemana,
                    Horario = turma.Horario
                },
                Mes = inicioDoMes.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                Datas = datasDeAula.Select(dataDeAula => new ChamadaMensalDataDeAulaDTO
                {
                    Data = dataDeAula,
                    PodeEditar = AvaliarEdicao(turma, dataDeAula, ehCoordenador).podeEditar
                }).ToList(),
                Alunos = alunos.Select(aluno => new ChamadaMensalAlunoDTO
                {
                    AlunoId = aluno.Id,
                    NomeCompleto = aluno.NomeCompleto,
                    EhInadimplente = aluno.Status == StatusAluno.Inadimplente,
                    Presencas = datasDeAula.Select(dataDeAula => new ChamadaMensalPresencaDTO
                    {
                        Data = dataDeAula,
                        Presente = presentePorAlunoEData.TryGetValue((aluno.Id, dataDeAula), out var presente) ? presente : (bool?)null
                    }).ToList()
                }).ToList()
            };

            string mensagem = datasDeAula.Count == 0
                ? "A turma não tem aulas neste mês."
                : $"{Plural(datasDeAula.Count, "aula", "aulas")} no mês.";

            return new ChamadaMensalResponseDTO { Status = true, Message = mensagem, Data = data };
        }

        private async Task<(Turma? turma, ChamadaResponseDTO? erro)> ValidarAcessoAsync(Guid turmaId, string nomeUsuario, bool ehCoordenador)
        {
            var turma = await _presencaRepository.ObterTurmaPorIdAsync(turmaId);
            if (turma == null)
                return (null, Falha("Turma não encontrada.", TipoFalhaPresenca.NaoEncontrado));

            if (ehCoordenador)
                return (turma, null);

            var professor = await _presencaRepository.ObterProfessorPorNomeUsuarioAsync(nomeUsuario);
            if (professor == null || turma.ProfessorId != professor.Id)
                return (null, Falha("Você não tem acesso a esta turma.", TipoFalhaPresenca.AcessoNegado));

            return (turma, null);
        }

        private (bool podeEditar, string? motivo) AvaliarEdicao(Turma turma, DateOnly data, bool ehCoordenador)
        {
            var hoje = _relogio.HojeNoBrasil();

            if (data > hoje)
                return (false, "Não é possível lançar chamada em data futura.");

            if (!turma.TemAulaEm(data))
                return (false, "A turma não tem aula nesta data.");

            var limite = _configuracoes.DiasLimiteEdicaoProfessor;
            if (!ehCoordenador && hoje.DayNumber - data.DayNumber > limite)
                return (false, $"O professor só pode alterar chamadas dos últimos {limite} dias. Peça à Coordenação para corrigir.");

            return (true, null);
        }

        private static ChamadaDataDTO MontarChamada(Turma turma, DateOnly data, List<Aluno> alunos, List<Presenca> presencas, bool podeEditar, string? motivoSomenteLeitura)
        {
            var presentePorAluno = presencas
                .GroupBy(p => p.AlunoId)
                .ToDictionary(g => g.Key, g => g.First().Presente);

            return new ChamadaDataDTO
            {
                TurmaId = turma.Id,
                TurmaNome = turma.Nome,
                Data = data,
                PodeEditar = podeEditar,
                MotivoSomenteLeitura = motivoSomenteLeitura,
                Alunos = alunos.Select(a => new ChamadaAlunoDTO
                {
                    AlunoId = a.Id,
                    NomeCompleto = a.NomeCompleto,
                    FotoUrl = a.FotoUrl,
                    NivelTecnico = a.NivelTecnico.ToString(),
                    EhInadimplente = a.Status == StatusAluno.Inadimplente,
                    Presente = presentePorAluno.TryGetValue(a.Id, out var presente) ? presente : (bool?)null
                }).ToList()
            };
        }

        private static string Plural(int quantidade, string singular, string plural)
        {
            return quantidade == 1 ? $"1 {singular}" : $"{quantidade} {plural}";
        }

        private static ChamadaResponseDTO Sucesso(string message, ChamadaDataDTO data) => new()
        {
            Status = true,
            Message = message,
            Data = data
        };

        private static ChamadaResponseDTO Falha(string message, TipoFalhaPresenca falha) => new()
        {
            Status = false,
            Message = message,
            Falha = falha
        };

        private static ChamadaMensalResponseDTO FalhaMensal(string message, TipoFalhaPresenca falha) => new()
        {
            Status = false,
            Message = message,
            Falha = falha
        };
    }
}