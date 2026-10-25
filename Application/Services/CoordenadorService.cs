using System.Net.Mail;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class CoordenadorService : ICoordenadorService
    {
        private readonly ICoordenadorRepository _coordenadorRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISecurityService _securityService;

        public CoordenadorService(
            ICoordenadorRepository coordenadorRepository,
            IUsuarioRepository usuarioRepository,
            ISecurityService securityService)
        {
            _coordenadorRepository = coordenadorRepository;
            _usuarioRepository = usuarioRepository;
            _securityService = securityService;
        }

        public async Task<IEnumerable<CoordenadorDataDTO>> ListarAsync()
        {
            var coordenadores = await _coordenadorRepository.ObterTodosAsync();
            return coordenadores.Select(MapToDataDTO);
        }

        public async Task<CoordenadorResponseDTO> ObterPorIdAsync(Guid id)
        {
            var coordenador = await _coordenadorRepository.ObterPorIdAsync(id);
            if (coordenador == null)
                return Falha("Coordenador não encontrado.");

            return Sucesso("Coordenador recuperado com sucesso.", coordenador);
        }

        public async Task<CoordenadorResponseDTO> CriarAsync(CriarCoordenadorRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return Falha("O nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.NomeUsuario))
                return Falha("O nome de usuário é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                return Falha("A senha é obrigatória.");

             if (!TentarNormalizarEmail(dto.Email, out var email))
                return Falha("E-mail inválido.");

            if (await _coordenadorRepository.EmailJaCadastradoAsync(email))
                return Falha("Já existe um usuário com este e-mail.");

   
            var nomeUsuario = dto.NomeUsuario.Trim();
            if (await _usuarioRepository.ObterPorNomeUsuarioAsync(nomeUsuario) != null)
                return Falha("Já existe um usuário com esse nome de usuário.");

            
            var senhaHash = _securityService.HashPassword(dto.Senha);
            var usuario = new Usuario(nomeUsuario, email, senhaHash, PerfilUsuario.Coordenador);
            await _usuarioRepository.AddAsync(usuario);

            var coordenador = new Coordenador(dto.Nome.Trim(), usuario.Id);
            await _coordenadorRepository.AdicionarAsync(coordenador);

            return new CoordenadorResponseDTO
            {
                Status = true,
                Message = "Coordenador criado com sucesso.",
                Data = new CoordenadorDataDTO
                {
                    Id = coordenador.Id,
                    Nome = coordenador.Nome,
                    NomeUsuario = usuario.NomeUsuario,
                    Email = usuario.Email
                }
            };
        }

        public async Task<CoordenadorResponseDTO> AtualizarAsync(Guid id, AtualizarCoordenadorRequestDTO dto)
        {
            var coordenador = await _coordenadorRepository.ObterPorIdAsync(id);
            if (coordenador == null)
                return Falha("Coordenador não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Nome))
                return Falha("O nome é obrigatório.");

            if (!TentarNormalizarEmail(dto.Email, out var email))
                return Falha("E-mail inválido.");

            if (await _coordenadorRepository.EmailJaCadastradoAsync(email, coordenador.UsuarioId))
                return Falha("Já existe um usuário com este e-mail.");

            coordenador.Atualizar(dto.Nome.Trim());
            coordenador.Usuario.AlterarEmail(email);
            await _coordenadorRepository.AtualizarAsync(coordenador);

            return Sucesso("Coordenador atualizado com sucesso.", coordenador);
        }

        public async Task<CoordenadorResponseDTO> RemoverAsync(Guid id)
        {
            var coordenador = await _coordenadorRepository.ObterPorIdAsync(id);
            if (coordenador == null)
                return Falha("Coordenador não encontrado.");

           
            await _coordenadorRepository.RemoverAsync(coordenador);

            return new CoordenadorResponseDTO
            {
                Status = true,
                Message = "Coordenador removido com sucesso."
            };
        }

       
        private static bool TentarNormalizarEmail(string? email, out string emailNormalizado)
        {
            emailNormalizado = (email ?? string.Empty).Trim().ToLowerInvariant();

            return MailAddress.TryCreate(emailNormalizado, out var endereco)
                   && endereco.Address == emailNormalizado;
        }

        private static CoordenadorDataDTO MapToDataDTO(Coordenador c) => new()
        {
            Id = c.Id,
            Nome = c.Nome,
            NomeUsuario = c.Usuario?.NomeUsuario ?? string.Empty,
            Email = c.Usuario?.Email ?? string.Empty
        };

        private static CoordenadorResponseDTO Sucesso(string message, Coordenador c) => new()
        {
            Status = true,
            Message = message,
            Data = MapToDataDTO(c)
        };

        private static CoordenadorResponseDTO Falha(string message) => new()
        {
            Status = false,
            Message = message
        };
    }
}