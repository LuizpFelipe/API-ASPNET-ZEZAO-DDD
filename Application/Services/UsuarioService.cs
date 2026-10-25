using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private const int TamanhoMinimoDaSenha = 6;

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICodigoRecuperacaoSenhaRepository _codigoRepository;
        private readonly ISecurityService _securityService;
        private readonly IUnitOfWork _unitOfWork;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            ICodigoRecuperacaoSenhaRepository codigoRepository,
            ISecurityService securityService,
            IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _codigoRepository = codigoRepository;
            _securityService = securityService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UsuarioResponseDTO> ResetarSenhaAsync(Guid usuarioId, ResetarSenhaUsuarioRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NovaSenha) || dto.NovaSenha.Length < TamanhoMinimoDaSenha)
            {
                return Falha($"A nova senha precisa ter pelo menos {TamanhoMinimoDaSenha} caracteres.");
            }

            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario == null)
            {
                return Falha("Usuário não encontrado.");
            }

            if (usuario.Perfil != PerfilUsuario.Professor)
            {
                return Falha("Só é possível redefinir a senha de professores.");
            }

            usuario.AlterarSenhaHash(_securityService.HashPassword(dto.NovaSenha));

            await _unitOfWork.ExecutarEmTransacaoAsync(async () =>
            {
                await _usuarioRepository.AtualizarAsync(usuario);
                await _codigoRepository.InvalidarPendentesDoUsuarioAsync(usuario.Id);
            });

            return new UsuarioResponseDTO { Status = true, Message = "Senha redefinida com sucesso." };
        }

        private static UsuarioResponseDTO Falha(string mensagem)
        {
            return new UsuarioResponseDTO { Status = false, Message = mensagem };
        }
    }
}