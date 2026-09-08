using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class ProfessorService : IProfessorService
    {
        private readonly IProfessorRepository _professorRepository;


        public ProfessorService(IProfessorRepository professorRepository)
        {
            _professorRepository = professorRepository;
        }

        public async Task<IEnumerable<ProfessorResponseDTO>> ListarAsync()
        {
            var professores = await _professorRepository.ObterTodosAsync();

            // Mapeia cada entidade encontrada para o DTO de resposta correspondente
            var responseList = professores.Select(professor => new ProfessorResponseDTO
            {
                Status = true,
                Data = professor
            });

            return responseList;
        }

        public async Task<ProfessorResponseDTO> ObterPorIdAsync(Guid id)
        {
            var professor = await _professorRepository.ObterPorIdAsync(id);
            if (professor == null) return new ProfessorResponseDTO { Status = false, Message = "Não encontrado" };

            return new ProfessorResponseDTO { Status = true, Data = professor };
        }

        public async Task<ProfessorResponseDTO> CriarAsync(CriarProfessorRequestDTO dto)
        {


            var usuarioIdFicticio = Guid.NewGuid(); 
            var professor = new Professor(dto.Nome, dto.Telefone, usuarioIdFicticio);

            await _professorRepository.AdicionarAsync(professor);

            return new ProfessorResponseDTO { Status = true, Message = "Professor criado com sucesso", Data = professor };
        }

        public async Task<ProfessorResponseDTO> AtualizarAsync(Guid id, AtualizarProfessorRequestDTO dto)
        {
            var professor = await _professorRepository.ObterPorIdAsync(id);
            if (professor == null) return new ProfessorResponseDTO { Status = false, Message = "Professor não encontrado" };

            professor.AtualizarDetalhes(dto.Nome, dto.Telefone);
            await _professorRepository.AtualizarAsync(professor);

            return new ProfessorResponseDTO { Status = true, Message = "Atualizado com sucesso", Data = professor };
        }

        public async Task<ProfessorResponseDTO> RemoverAsync(Guid id)
        {
            var professor = await _professorRepository.ObterPorIdAsync(id);
            if (professor == null) return new ProfessorResponseDTO { Status = false, Message = "Professor não encontrado" };

            // Ponto de atenção: Verificar vínculo com turmas antes de remover, 
            // ou alterar status do Usuario vinculado para Inativo.
            await _professorRepository.RemoverAsync(professor);

            return new ProfessorResponseDTO { Status = true, Message = "Removido com sucesso" };
        }
    }
}