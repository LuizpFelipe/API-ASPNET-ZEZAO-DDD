using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class TurmaService : ITurmaService
    {
        private readonly ITurmaRepository _turmaRepository;

        public TurmaService(ITurmaRepository turmaRepository)
        {
            _turmaRepository = turmaRepository;
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
            var turma = new Turma(dto.Nome, dto.DiasSemana, dto.Horario, dto.CategoriaId, dto.ProfessorId);
            await _turmaRepository.AdicionarAsync(turma);
            return new TurmaResponseDTO { Status = true, Message = "Turma criada com sucesso", Data = turma };
        }

        public async Task<TurmaResponseDTO> AtualizarAsync(Guid id, AtualizarTurmaRequestDTO dto)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null) return new TurmaResponseDTO { Status = false, Message = "Turma não encontrada" };

            turma.AtualizarDetalhes(dto.Nome, dto.DiasSemana, dto.Horario, dto.CategoriaId, dto.ProfessorId);
            await _turmaRepository.AtualizarAsync(turma);

            return new TurmaResponseDTO { Status = true, Message = "Atualizada com sucesso", Data = turma };
        }

        public async Task<TurmaResponseDTO> RemoverAsync(Guid id)
        {
            var turma = await _turmaRepository.ObterPorIdAsync(id);
            if (turma == null) return new TurmaResponseDTO { Status = false, Message = "Turma não encontrada" };

            await _turmaRepository.RemoverAsync(turma);
            return new TurmaResponseDTO { Status = true, Message = "Removida com sucesso" };
        }
    }
}