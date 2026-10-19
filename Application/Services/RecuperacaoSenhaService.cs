using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class RecuperacaoSenhaService : IRecuperacaoSenhaService
    {
        private const int ValidadeDoCodigoEmMinutos = 5;
        private const int ValidadeDoTokenEmMinutos = 10;
        private const int MaximoDeTentativas = 5;
        private const int SegundosMinimosEntreEnvios = 60;
        private const int TamanhoMinimoDaSenha = 6;

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICodigoRecuperacaoSenhaRepository _codigoRepository;
        private readonly ISecurityService _securityService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public RecuperacaoSenhaService(
            IUsuarioRepository usuarioRepository,
            ICodigoRecuperacaoSenhaRepository codigoRepository,
            ISecurityService securityService,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
            _codigoRepository = codigoRepository;
            _securityService = securityService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RecuperacaoSenhaResponseDTO> SolicitarCodigoAsync(SolicitarRecuperacaoSenhaRequestDTO dto)
        {
            if (!Usuario.TentarNormalizarEmail(dto.Email, out var emailNormalizado))
            {
                return Falha("Informe um e-mail válido.");
            }

            var usuario = await _usuarioRepository.ObterPorEmailAsync(emailNormalizado);
            if (usuario == null)
            {
                return RespostaGenericaDeEnvio();
            }

            var agora = DateTime.UtcNow;
            var ultimoCodigo = await _codigoRepository.ObterMaisRecenteDoUsuarioAsync(usuario.Id);
            if (ultimoCodigo != null && ultimoCodigo.CriadoEm > agora.AddSeconds(-SegundosMinimosEntreEnvios))
            {
                return RespostaGenericaDeEnvio();
            }

            var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var novoRegistro = new CodigoRecuperacaoSenha(
                usuario.Id,
                _securityService.HashPassword(codigo),
                agora.AddMinutes(ValidadeDoCodigoEmMinutos));

            await _unitOfWork.ExecutarEmTransacaoAsync(async () =>
            {
                await _codigoRepository.InvalidarPendentesDoUsuarioAsync(usuario.Id);
                await _codigoRepository.AdicionarAsync(novoRegistro);
            });

            try
            {
                await _emailService.EnviarAsync(
                    usuario.Email,
                    "Código de recuperação de senha - Zezão Futsal",
                    $"Olá, {usuario.NomeUsuario}.\n\n" +
                    $"Seu código de recuperação de senha é: {codigo}\n\n" +
                    $"Ele vale por {ValidadeDoCodigoEmMinutos} minutos. " +
                    "Se você não pediu a recuperação, ignore este e-mail.");
            }
            catch (Exception)
            {
                return Falha("Não foi possível enviar o e-mail agora. Tente novamente em instantes.");
            }

            return RespostaGenericaDeEnvio();
        }

        public async Task<RecuperacaoSenhaResponseDTO> ValidarCodigoAsync(ValidarCodigoRecuperacaoRequestDTO dto)
        {
            const string mensagemDeFalha = "Código inválido ou expirado.";

            if (!Usuario.TentarNormalizarEmail(dto.Email, out var emailNormalizado)
                || string.IsNullOrWhiteSpace(dto.Codigo))
            {
                return Falha(mensagemDeFalha);
            }

            var usuario = await _usuarioRepository.ObterPorEmailAsync(emailNormalizado);
            if (usuario == null)
            {
                return Falha(mensagemDeFalha);
            }

            var agora = DateTime.UtcNow;
            var registro = await _codigoRepository.ObterMaisRecenteDoUsuarioAsync(usuario.Id);
            if (registro == null || !registro.PodeValidarCodigo(agora, MaximoDeTentativas))
            {
                return Falha(mensagemDeFalha);
            }

            if (!_securityService.VerifyPassword(dto.Codigo.Trim(), registro.CodigoHash))
            {
                registro.RegistrarTentativaInvalida();
                await _codigoRepository.AtualizarAsync(registro);
                return Falha(mensagemDeFalha);
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            registro.MarcarCodigoValidado(GerarHashDoToken(token), agora.AddMinutes(ValidadeDoTokenEmMinutos));
            await _codigoRepository.AtualizarAsync(registro);

            return new RecuperacaoSenhaResponseDTO
            {
                Status = true,
                Message = "Código validado com sucesso.",
                Data = new TokenRedefinicaoDataDTO { Token = token }
            };
        }

        public async Task<RecuperacaoSenhaResponseDTO> RedefinirSenhaAsync(RedefinirSenhaRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NovaSenha) || dto.NovaSenha.Length < TamanhoMinimoDaSenha)
            {
                return Falha($"A nova senha precisa ter pelo menos {TamanhoMinimoDaSenha} caracteres.");
            }

            if (string.IsNullOrWhiteSpace(dto.Token))
            {
                return Falha("Solicitação inválida ou expirada. Peça um novo código.");
            }

            var registro = await _codigoRepository.ObterPorTokenHashAsync(GerarHashDoToken(dto.Token));
            if (registro == null || !registro.PodeRedefinirSenha(DateTime.UtcNow))
            {
                return Falha("Solicitação inválida ou expirada. Peça um novo código.");
            }

            var usuario = await _usuarioRepository.ObterPorIdAsync(registro.UsuarioId);
            if (usuario == null)
            {
                return Falha("Solicitação inválida ou expirada. Peça um novo código.");
            }

            if (_securityService.VerifyPassword(dto.NovaSenha, usuario.SenhaHash))
            {
                return Falha("A nova senha deve ser diferente da atual.");
            }

            usuario.AlterarSenhaHash(_securityService.HashPassword(dto.NovaSenha));
            registro.MarcarComoUsado();

            await _unitOfWork.ExecutarEmTransacaoAsync(async () =>
            {
                await _usuarioRepository.AtualizarAsync(usuario);
                await _codigoRepository.AtualizarAsync(registro);
            });

            return new RecuperacaoSenhaResponseDTO { Status = true, Message = "Senha alterada com sucesso." };
        }

        private static string GerarHashDoToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        private static RecuperacaoSenhaResponseDTO RespostaGenericaDeEnvio()
        {
            return new RecuperacaoSenhaResponseDTO
            {
                Status = true,
                Message = "Se o e-mail estiver cadastrado, enviamos um código."
            };
        }

        private static RecuperacaoSenhaResponseDTO Falha(string mensagem)
        {
            return new RecuperacaoSenhaResponseDTO { Status = false, Message = mensagem };
        }
    }
}