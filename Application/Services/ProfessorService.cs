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
        private const int TamanhoMinimoDaSenha = 6;
        private const string MensagemEmailInvalido = "E-mail inválido.";
        private const string MensagemEmailJaCadastrado = "Já existe um usuário com este e-mail.";
        private const string MensagemNomeUsuarioJaCadastrado = "Já existe um usuário com esse nome de usuário.";

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
            var professores = (await _professorRepository.ObterTodosAsync()).ToList();

            var usuarios = await _usuarioRepository.ObterPorIdsAsync(
                professores.Select(professor => professor.UsuarioId));
            var usuariosPorIdentificador = usuarios.ToDictionary(usuario => usuario.Id);

            return professores.Select(professor =>
            {
                usuariosPorIdentificador.TryGetValue(professor.UsuarioId, out var usuario);

                return new ProfessorResponseDTO
                {
                    Status = true,
                    Data = MontarDados(professor, usuario)
                };
            }).ToList();
        }

        public async Task<ProfessorResponseDTO> ObterPorIdAsync(Guid id)
        {
            var professor = await _professorRepository.ObterPorIdAsync(id);
            if (professor == null) return new ProfessorResponseDTO { Status = false, Message = "Não encontrado" };

            var usuario = await _usuarioRepository.ObterPorIdAsync(professor.UsuarioId);

            return new ProfessorResponseDTO { Status = true, Data = MontarDados(professor, usuario) };
        }

        public async Task<ProfessorResponseDTO> CriarAsync(CriarProfessorRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return Falha("O nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.NomeUsuario))
                return Falha("O nome de usuário é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                return Falha("A senha é obrigatória.");

            if (dto.Senha.Length < TamanhoMinimoDaSenha)
                return Falha($"A senha precisa ter pelo menos {TamanhoMinimoDaSenha} caracteres.");

            if (string.IsNullOrWhiteSpace(dto.Telefone))
                return Falha("O telefone é obrigatório.");

            if (!Usuario.TentarNormalizarEmail(dto.Email, out var email))
                return Falha(MensagemEmailInvalido);

            if (await _usuarioRepository.ObterPorEmailAsync(email) != null)
                return Falha(MensagemEmailJaCadastrado);

            var nomeUsuario = dto.NomeUsuario.Trim();
            if (await _usuarioRepository.ObterPorNomeUsuarioAsync(nomeUsuario) != null)
                return Falha(MensagemNomeUsuarioJaCadastrado);

            var senhaHash = _securityService.HashPassword(dto.Senha);
            var usuario = new Usuario(nomeUsuario, email, senhaHash, PerfilUsuario.Professor);
            await _usuarioRepository.AddAsync(usuario);

            var professor = new Professor(dto.Nome.Trim(), dto.Telefone, usuario.Id);
            await _professorRepository.AdicionarAsync(professor);

            return new ProfessorResponseDTO
            {
                Status = true,
                Message = "Professor criado com sucesso",
                Data = MontarDados(professor, usuario)
            };
        }

        public async Task<ProfessorResponseDTO> AtualizarAsync(Guid id, AtualizarProfessorRequestDTO dto)
        {
            var professor = await _professorRepository.ObterPorIdAsync(id);
            if (professor == null) return Falha("Professor não encontrado");

            if (string.IsNullOrWhiteSpace(dto.Nome))
                return Falha("O nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Telefone))
                return Falha("O telefone é obrigatório.");

            if (!Usuario.TentarNormalizarEmail(dto.Email, out var email))
                return Falha(MensagemEmailInvalido);

            var usuario = await _usuarioRepository.ObterPorIdAsync(professor.UsuarioId);
            if (usuario == null) return Falha("Usuário do professor não encontrado");

            var usuarioComMesmoEmail = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuarioComMesmoEmail != null && usuarioComMesmoEmail.Id != usuario.Id)
                return Falha(MensagemEmailJaCadastrado);

            usuario.AlterarEmail(email);
            await _usuarioRepository.AtualizarAsync(usuario);

            professor.AtualizarDetalhes(dto.Nome.Trim(), dto.Telefone);
            await _professorRepository.AtualizarAsync(professor);

            return new ProfessorResponseDTO
            {
                Status = true,
                Message = "Atualizado com sucesso",
                Data = MontarDados(professor, usuario)
            };
        }

        public async Task<ProfessorResponseDTO> RemoverAsync(Guid id)
        {
            var professor = await _professorRepository.ObterPorIdAsync(id);
            if (professor == null) return Falha("Professor não encontrado");

            if (await _professorRepository.PossuiTurmasAsync(professor.Id))
                return Falha("Não é possível remover o professor: existem turmas vinculadas a ele.");

            var usuario = await _usuarioRepository.ObterPorIdAsync(professor.UsuarioId);

            await _professorRepository.RemoverAsync(professor);

            if (usuario != null)
                await _usuarioRepository.RemoverAsync(usuario);

            return new ProfessorResponseDTO { Status = true, Message = "Removido com sucesso" };
        }

        private static ProfessorResponseDTO Falha(string mensagem)
        {
            return new ProfessorResponseDTO { Status = false, Message = mensagem };
        }

        private static ProfessorDadosDTO MontarDados(Professor professor, Usuario? usuario)
        {
            return new ProfessorDadosDTO
            {
                Id = professor.Id,
                Nome = professor.Nome,
                Telefone = professor.Telefone,
                UsuarioId = professor.UsuarioId,
                NomeUsuario = usuario?.NomeUsuario ?? string.Empty,
                Email = usuario?.Email ?? string.Empty
            };
        }
    }
}