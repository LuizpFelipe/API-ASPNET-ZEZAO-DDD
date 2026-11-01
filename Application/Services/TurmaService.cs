using Application.Interfaces;
using Application.Request;
using Application.Response;
using Application.Validacoes;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Utilitarios;

namespace Application.Services
{
    public class TurmaService : ITurmaService
    {
        private const string MensagemDiasInvalidos = "Informe ao menos um dia da semana válido. Exemplo: Terça,Quinta.";

        private readonly ITurmaRepository _turmaRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IProfessorRepository _professorRepository;

        public TurmaService(
            ITurmaRepository turmaRepository,
            ICategoriaRepository categoriaRepository,
            IProfessorRepository professorRepository)
        {
            _turmaRepository = turmaRepository;
            _categoriaRepository = categoriaRepository;
            _professorRepository = professorRepository;
        }

        public async Task<IEnumerable<TurmaResponseDTO>> ListarAsync()
        {
            var turmas = await _turmaRepository.ObterTodasAsync();

            var responseList = new List<TurmaResponseDTO>();
            foreach (var turma in turmas)
            {
                responseList.Add(new TurmaResponseDTO
                {
                    Status = true,
                    Data = turma
                });
            }

            return responseList;
        }

        public async Task<TurmaResponseDTO> ObterPorIdAsync(Guid id)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null) return new TurmaResponseDTO { Status = false, Message = "Turma não encontrada" };
            return new TurmaResponseDTO { Status = true, Data = turma };
        }

        public async Task<IEnumerable<TurmaResponseDTO>> ListarPorCategoriaAsync(Guid categoriaId)
        {
            throw new NotImplementedException();
        }

        public async Task<TurmaResponseDTO> CriarAsync(CriarTurmaRequestDTO dto)
        {
            var mensagemDeDadosInvalidos = ValidarNomeEHorario(dto.Nome, dto.Horario);
            if (mensagemDeDadosInvalidos != null)
                return new TurmaResponseDTO { Status = false, Message = mensagemDeDadosInvalidos };

            if (!DiasDaSemana.EhValido(dto.DiasSemana))
                return new TurmaResponseDTO { Status = false, Message = MensagemDiasInvalidos };

            var mensagemDeErro = await ValidarCategoriaEProfessorAsync(dto.CategoriaId, dto.ProfessorId);
            if (mensagemDeErro != null)
                return new TurmaResponseDTO { Status = false, Message = mensagemDeErro };

            var mensagemDeConflito = await VerificarConflitoAsync(null, dto.Nome, dto.DiasSemana, dto.Horario, dto.CategoriaId, dto.ProfessorId);
            if (mensagemDeConflito != null)
                return new TurmaResponseDTO { Status = false, Message = mensagemDeConflito };

            var turma = new Turma(dto.Nome, DiasDaSemana.Padronizar(dto.DiasSemana), dto.Horario, dto.CategoriaId, dto.ProfessorId);
            await _turmaRepository.AdicionarAsync(turma);
            return new TurmaResponseDTO { Status = true, Message = "Turma criada com sucesso", Data = turma };
        }

        public async Task<TurmaResponseDTO> AtualizarAsync(Guid id, AtualizarTurmaRequestDTO dto)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null) return new TurmaResponseDTO { Status = false, Message = "Turma não encontrada" };

            var mensagemDeDadosInvalidos = ValidarNomeEHorario(dto.Nome, dto.Horario);
            if (mensagemDeDadosInvalidos != null)
                return new TurmaResponseDTO { Status = false, Message = mensagemDeDadosInvalidos };

            if (!DiasDaSemana.EhValido(dto.DiasSemana))
                return new TurmaResponseDTO { Status = false, Message = MensagemDiasInvalidos };

            var mensagemDeErro = await ValidarCategoriaEProfessorAsync(dto.CategoriaId, dto.ProfessorId);
            if (mensagemDeErro != null)
                return new TurmaResponseDTO { Status = false, Message = mensagemDeErro };

            var mensagemDeConflito = await VerificarConflitoAsync(id, dto.Nome, dto.DiasSemana, dto.Horario, dto.CategoriaId, dto.ProfessorId);
            if (mensagemDeConflito != null)
                return new TurmaResponseDTO { Status = false, Message = mensagemDeConflito };

            turma.AtualizarDetalhes(dto.Nome, DiasDaSemana.Padronizar(dto.DiasSemana), dto.Horario, dto.CategoriaId, dto.ProfessorId);
            await _turmaRepository.AtualizarAsync(turma);

            return new TurmaResponseDTO { Status = true, Message = "Atualizada com sucesso", Data = turma };
        }

        public async Task<TurmaResponseDTO> RemoverAsync(Guid id)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null) return new TurmaResponseDTO { Status = false, Message = "Turma não encontrada" };

            if (await _turmaRepository.PossuiVinculosAsync(id))
                return new TurmaResponseDTO
                {
                    Status = false,
                    Message = "Não é possível remover a turma: existem alunos ou presenças vinculados a ela."
                };

            await _turmaRepository.RemoverAsync(turma);
            return new TurmaResponseDTO { Status = true, Message = "Removida com sucesso" };
        }

        public async Task<TurmaResponseDTO> InativarAsync(Guid id, Guid? turmaDestinoId = null)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null) return new TurmaResponseDTO { Status = false, Message = "Turma não encontrada" };

            if (!turma.Ativa)
                return new TurmaResponseDTO { Status = false, Message = "A turma já está inativa." };

            var alunosAtivos = await _turmaRepository.ContarAlunosNaoInativosAsync(id);
            Guid? destinoParaTransferir = null;
            if (alunosAtivos > 0)
            {
                if (!turmaDestinoId.HasValue)
                    return new TurmaResponseDTO
                    {
                        Status = false,
                        Message = $"A turma ainda tem {alunosAtivos} aluno(s) ativo(s). Escolha a turma que vai recebê-los."
                    };

                var turmaDestino = await _turmaRepository.ObterPorIdAsync(turmaDestinoId.Value);
                if (turmaDestino == null || turmaDestino.Id == id)
                    return new TurmaResponseDTO { Status = false, Message = "Turma de destino inválida." };
                if (!turmaDestino.Ativa)
                    return new TurmaResponseDTO { Status = false, Message = "A turma de destino está inativa." };
                if (turmaDestino.CategoriaId != turma.CategoriaId)
                    return new TurmaResponseDTO { Status = false, Message = "A turma de destino precisa ser da mesma categoria." };

                destinoParaTransferir = turmaDestino.Id;
            }

            turma.Inativar();
            await _turmaRepository.SalvarInativacaoAsync(turma, destinoParaTransferir);
            return new TurmaResponseDTO { Status = true, Message = "Turma inativada com sucesso", Data = turma };
        }

        public async Task<TurmaResponseDTO> ReativarAsync(Guid id)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null) return new TurmaResponseDTO { Status = false, Message = "Turma não encontrada" };

            if (turma.Ativa)
                return new TurmaResponseDTO { Status = false, Message = "A turma já está ativa." };

            var mensagemDeConflito = await VerificarConflitoAsync(id, turma.Nome, turma.DiasSemana, turma.Horario, turma.CategoriaId, turma.ProfessorId);
            if (mensagemDeConflito != null)
                return new TurmaResponseDTO { Status = false, Message = mensagemDeConflito };

            turma.Reativar();
            await _turmaRepository.AtualizarAsync(turma);
            return new TurmaResponseDTO { Status = true, Message = "Turma reativada com sucesso", Data = turma };
        }

        private static string? ValidarNomeEHorario(string? nome, string? horario)
        {
            if (string.IsNullOrWhiteSpace(nome)) return "Informe o nome da turma.";
            if (!HorarioValidador.EhValido(horario)) return "Horário inválido. Use um formato como 17h, 17h30 ou 17:30.";

            return null;
        }

        private async Task<string?> VerificarConflitoAsync(Guid? ignorarId, string nome, string diasSemana, string horario, Guid categoriaId, Guid professorId)
        {
            var turmas = (await _turmaRepository.ObterTodasAsync()).Where(turma => turma.Id != ignorarId && turma.Ativa).ToList();
            var diasInformados = DiasDaSemana.Converter(DiasDaSemana.Padronizar(diasSemana));

            if (turmas.Any(turma => turma.CategoriaId == categoriaId
                                    && string.Equals(turma.Nome?.Trim(), nome?.Trim(), StringComparison.OrdinalIgnoreCase)))
                return "Já existe uma turma com esse nome nesta categoria.";

            if (turmas.Any(turma => turma.ProfessorId == professorId
                                    && string.Equals(turma.Horario?.Trim(), horario?.Trim(), StringComparison.OrdinalIgnoreCase)
                                    && DiasDaSemana.Converter(turma.DiasSemana).Any(dia => diasInformados.Contains(dia))))
                return "O professor já tem uma turma neste horário e dia da semana.";

            return null;
        }

        private async Task<string?> ValidarCategoriaEProfessorAsync(Guid categoriaId, Guid professorId)
        {
            if (await _categoriaRepository.ObterPorIdAsync(categoriaId) == null)
                return "Categoria não encontrada.";

            if (await _professorRepository.ObterPorIdAsync(professorId) == null)
                return "Professor não encontrado.";

            return null;
        }
    }
}