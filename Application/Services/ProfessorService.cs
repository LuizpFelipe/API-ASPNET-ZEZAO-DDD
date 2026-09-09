using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class ProfessorService : IProfessorService
    {
        private readonly IProfessorRepository _professorRepository;
        private readonly IUserRepository _userRepository; // Utilizando o repositório em inglês
        private readonly ISecurityService _securityService;

        public ProfessorService(
            IProfessorRepository professorRepository,
            IUserRepository userRepository,
            ISecurityService securityService)
        {
            _professorRepository = professorRepository;
            _userRepository = userRepository;
            _securityService = securityService;
        }

        public async Task<IEnumerable<ProfessorResponseDTO>> ListarAsync()
        {
            var professores = await _professorRepository.ObterTodosAsync();

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
            // 1. Gera o hash da senha
            var senhaHash = _securityService.HashPassword(dto.Senha);

            // 2. Cria a entidade User (adaptando NomeUsuario para o campo de Email/Login)
            var user = new User(dto.Nome, dto.NomeUsuario, senhaHash);

            // 3. Salva o User no banco de dados real
            await _userRepository.AddAsync(user);

            // 4. Cria o Professor vinculando com o Id do User recém-criado
            var professor = new Professor(dto.Nome, dto.Telefone, user.Id);
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
            // ou alterar status do User vinculado para Inativo.
            await _professorRepository.RemoverAsync(professor);

            return new ProfessorResponseDTO { Status = true, Message = "Removido com sucesso" };
        }
    }
}