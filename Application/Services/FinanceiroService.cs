using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class FinanceiroService : IFinanceiroService
    {
        private readonly IParcelaRepository _parcelaRepository;
        private readonly IAvisoCobrancaRepository _avisoRepository;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IRelogio _relogio;

        public FinanceiroService(
            IParcelaRepository parcelaRepository,
            IAvisoCobrancaRepository avisoRepository,
            IWhatsAppService whatsAppService,
            IRelogio relogio)
        {
            _parcelaRepository = parcelaRepository;
            _avisoRepository = avisoRepository;
            _whatsAppService = whatsAppService;
            _relogio = relogio;
        }

        public async Task<IEnumerable<object>> ListarParcelasAsync(Guid? alunoId, string? status, int? mes)
        {
            var hoje = _relogio.HojeNoBrasil();
            var parcelas = await _parcelaRepository.ObterDetalhadasAsync(alunoId, status, mes, hoje);

            return parcelas.Select(item => MontarParcela(item, hoje)).ToList();
        }

        public async Task<ParcelaResponseDTO> RegistrarPagamentoAsync(Guid parcelaId)
        {
            var parcela = await _parcelaRepository.ObterPorIdAsync(parcelaId);
            if (parcela == null)
                return new ParcelaResponseDTO { Status = false, Message = "Parcela não encontrada." };

            if (parcela.Status == StatusParcela.Pago)
                return new ParcelaResponseDTO { Status = false, Message = "Esta parcela já se encontra paga." };

            parcela.MarcarComoPaga(_relogio.HojeNoBrasil());

            await _parcelaRepository.AtualizarAsync(parcela);

            return new ParcelaResponseDTO
            {
                Status = true,
                Message = "Pagamento registrado com sucesso.",
                Data = new
                {
                    parcela.Id,
                    parcela.ContratoId,
                    parcela.Numero,
                    Vencimento = parcela.DataVencimento,
                    parcela.Valor,
                    Status = "Pago",
                    parcela.DataPagamento
                }
            };
        }

        public async Task<ParcelaResponseDTO> DispararCobrancaAsync(Guid parcelaId)
        {
            var item = await _parcelaRepository.ObterDetalhadaPorIdAsync(parcelaId);
            if (item == null)
                return new ParcelaResponseDTO { Status = false, Message = "Parcela não encontrada." };

            if (item.Parcela.Status == StatusParcela.Pago)
                return new ParcelaResponseDTO { Status = false, Message = "Esta parcela já foi paga. Não há cobrança para enviar." };

            if (item.StatusContrato == StatusContrato.Cancelado)
                return new ParcelaResponseDTO { Status = false, Message = "O contrato desta parcela está cancelado. Não é possível enviar cobrança." };

            var telefone = NormalizarTelefone(item.Telefone);
            if (telefone == null)
                return new ParcelaResponseDTO { Status = false, Message = "O responsável deste aluno não tem um telefone válido cadastrado." };

            var hoje = _relogio.HojeNoBrasil();
            var atrasada = item.Parcela.DataVencimento < hoje;

            var enviado = await _whatsAppService.EnviarMensagemAsync(telefone, MontarMensagem(item, atrasada));
            if (!enviado)
                return new ParcelaResponseDTO { Status = false, Message = "Falha ao enviar a cobrança via WhatsApp." };

            var tipoAviso = atrasada ? TipoAvisoCobranca.AvisoAtraso : TipoAvisoCobranca.AvisoPrevio;
            await _avisoRepository.AdicionarAsync(new AvisoCobranca(item.Parcela.Id, tipoAviso, telefone));

            return new ParcelaResponseDTO { Status = true, Message = "Cobrança disparada via WhatsApp com sucesso." };
        }

        public async Task<ResumoFinanceiroResponseDTO> ObterResumoAsync()
        {
            var hoje = _relogio.HojeNoBrasil();
            var inicioDoMes = new DateOnly(hoje.Year, hoje.Month, 1);
            var inicioDoProximoMes = inicioDoMes.AddMonths(1);

            var parcelas = await _parcelaRepository.ObterDetalhadasAsync(null, null, null, hoje);

            decimal recebidoMes = 0;
            decimal pendente = 0;
            decimal inadimplencia = 0;

            foreach (var item in parcelas)
            {
                var parcela = item.Parcela;

                if (parcela.Status == StatusParcela.Pago)
                {
                    if (parcela.DataPagamento.HasValue
                        && parcela.DataPagamento.Value >= inicioDoMes
                        && parcela.DataPagamento.Value < inicioDoProximoMes)
                    {
                        recebidoMes += parcela.Valor;
                    }
                }
                else if (parcela.DataVencimento < hoje)
                {
                    inadimplencia += parcela.Valor;
                }
                else
                {
                    pendente += parcela.Valor;
                }
            }

            return new ResumoFinanceiroResponseDTO
            {
                Status = true,
                Message = "Resumo financeiro obtido com sucesso.",
                Data = new ResumoFinanceiroData
                {
                    RecebidoMes = recebidoMes,
                    Pendente = pendente,
                    Inadimplencia = inadimplencia
                }
            };
        }

        private static string StatusEfetivo(Parcela parcela, DateOnly hoje)
        {
            if (parcela.Status == StatusParcela.Pago)
                return "Pago";

            return parcela.DataVencimento < hoje ? "Atrasado" : "Pendente";
        }

        private static object MontarParcela(ParcelaDetalhada item, DateOnly hoje)
        {
            var parcela = item.Parcela;

            return new
            {
                parcela.Id,
                parcela.ContratoId,
                item.AlunoId,
                Aluno = item.AlunoNome,
                parcela.Numero,
                Vencimento = parcela.DataVencimento,
                parcela.Valor,
                Status = StatusEfetivo(parcela, hoje),
                parcela.DataPagamento
            };
        }

        private static string? NormalizarTelefone(string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return null;

            var digitos = new string(telefone.Where(char.IsDigit).ToArray());

            if (digitos.StartsWith("55") && (digitos.Length == 12 || digitos.Length == 13))
                return digitos;

            if (digitos.Length == 10 || digitos.Length == 11)
                return "55" + digitos;

            return null;
        }

        private static string MontarMensagem(ParcelaDetalhada item, bool atrasada)
        {
            var valor = item.Parcela.Valor.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
            var vencimento = item.Parcela.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var situacao = atrasada
                ? $"venceu em {vencimento} e ainda não foi paga"
                : $"vence em {vencimento}";

            return $"Olá, {item.ResponsavelNome}! Constatamos a parcela {item.Parcela.Numero} do aluno {item.AlunoNome}, no valor de R$ {valor}, que {situacao}. Por favor, regularize o pagamento. ZezaoFutsal";
        }
    }
}