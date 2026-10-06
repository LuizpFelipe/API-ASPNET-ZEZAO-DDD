using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class ProfessorService : IProfessorService
    {
        private readonly IProfessorRepository _professorRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISecurityService _securityService;

        public ProfessorService(
            IProfessorRepository professorRepository,
            IUsuarioRepository usuarioRepository,
            ISecurityService securityService)
        {
            _professorRepository = professorRepository;
            _usuarioRepository = usuarioRepository;
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
            var senhaHash = _securityService.HashPassword(dto.Senha);

            var usuario = new Usuario(dto.NomeUsuario, dto.Email, senhaHash, PerfilUsuario.Professor);

            await _usuarioRepository.AddAsync(usuario);

            var professor = new Professor(dto.Nome, dto.Telefone, usuario.Id);
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

            await _professorRepository.RemoverAsync(professor);

            return new ProfessorResponseDTO { Status = true, Message = "Removido com sucesso" };
        }
    }
}