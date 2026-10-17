using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class PresencaService : IPresencaService
    {
        private readonly IPresencaRepository _presencaRepository;

        public PresencaService(IPresencaRepository presencaRepository)
        {
            _presencaRepository = presencaRepository;
        }

        public async Task<TurmaDoProfessorResponseDTO> ListarTurmasDoProfessorAsync(string nomeUsuario, bool ehCoordenador)
        {
            // Coordenador vê todas as turmas; Professor só as dele.
            Guid? professorId = null;

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

            var turmas = await _presencaRepository.ObterTurmasAsync(professorId);

            var lista = turmas.Select(t => new TurmaDoProfessorDataDTO
            {
                Id = t.Id,
                Nome = t.Nome,
                DiasSemana = t.DiasSemana,
                Horario = t.Horario,
                CategoriaId = t.CategoriaId
            }).ToList();

            string mensagem;
            if (lista.Count == 0)
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

            var chamada = MontarChamada(turma!, data.Value, alunos, presencas);

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

            if (dto.Presencas == null || dto.Presencas.Count == 0)
                return Falha("Informe ao menos um aluno na lista de presenças.", TipoFalhaPresenca.Invalido);

            if (dto.Presencas.GroupBy(p => p.AlunoId).Any(g => g.Count() > 1))
                return Falha("Há aluno repetido na lista de presenças.", TipoFalhaPresenca.Invalido);

            // Só aceita alunos que pertencem a esta turma (e não estão inativos).
            var alunos = (await _presencaRepository.ObterAlunosDaTurmaAsync(turmaId)).ToList();
            var idsDaTurma = alunos.Select(a => a.Id).ToHashSet();

            if (dto.Presencas.Any(p => !idsDaTurma.Contains(p.AlunoId)))
                return Falha("Há aluno na lista que não pertence a esta turma ou está inativo.", TipoFalhaPresenca.Invalido);

            // Se já existe registro do aluno nessa data, atualiza; senão, cria.
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

            return Sucesso(mensagem, MontarChamada(turma!, dto.Data, alunos, existentes));
        }

        // ---------- helpers ----------

        // Garante que a turma existe e que o usuário pode acessá-la:
        // Coordenador acessa qualquer uma; Professor só as turmas dele.
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

        private static ChamadaDataDTO MontarChamada(Turma turma, DateOnly data, List<Aluno> alunos, List<Presenca> presencas)
        {
            var presentePorAluno = presencas
                .GroupBy(p => p.AlunoId)
                .ToDictionary(g => g.Key, g => g.First().Presente);

            return new ChamadaDataDTO
            {
                TurmaId = turma.Id,
                TurmaNome = turma.Nome,
                Data = data,
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
    }
}