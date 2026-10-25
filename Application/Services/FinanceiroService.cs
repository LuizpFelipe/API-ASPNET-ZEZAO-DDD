using System;
using System.Collections.Generic;
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

        public FinanceiroService(
            IParcelaRepository parcelaRepository,
            IAvisoCobrancaRepository avisoRepository,
            IWhatsAppService whatsAppService)
        {
            _parcelaRepository = parcelaRepository;
            _avisoRepository = avisoRepository;
            _whatsAppService = whatsAppService;
        }

        public async Task<IEnumerable<object>> ListarParcelasAsync(Guid? alunoId, string? status, int? mes)
        {
            var parcelas = await _parcelaRepository.ObterComFiltrosAsync(alunoId, status, mes);
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

            return parcelas.Select(p => new
            {
                p.Id,
                p.ContratoId,
                p.Numero,
                Vencimento = p.DataVencimento,
                p.Valor,
                Status = p.DataVencimento < hoje && p.Status == StatusParcela.Pendente ? "Atrasado" : p.Status.ToString(),
                p.DataPagamento
            });
        }

        public async Task<ParcelaResponseDTO> RegistrarPagamentoAsync(Guid parcelaId)
        {
            var parcela = await _parcelaRepository.ObterPorIdAsync(parcelaId);
            if (parcela == null)
                return new ParcelaResponseDTO { Status = false, Message = "Parcela não encontrada." };

            if (parcela.Status == StatusParcela.Pago)
                return new ParcelaResponseDTO { Status = false, Message = "Esta parcela já se encontra paga." };

            var dataHoje = DateOnly.FromDateTime(DateTime.UtcNow);
            parcela.MarcarComoPaga(dataHoje);

            await _parcelaRepository.AtualizarAsync(parcela);

            return new ParcelaResponseDTO { Status = true, Message = "Pagamento registrado com sucesso.", Data = parcela };
        }

        public async Task<ParcelaResponseDTO> DispararCobrancaAsync(Guid parcelaId)
        {
            var parcela = await _parcelaRepository.ObterPorIdAsync(parcelaId);
            if (parcela == null)
                return new ParcelaResponseDTO { Status = false, Message = "Parcela não encontrada." };

            // Telefone de exemplo do responsável (pode ser ajustado conforme a relação com o Contrato/Aluno)
            string telefoneResponsavel = "5544999999999";
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

            string mensagem = $"Olá! Constatamos uma parcela no valor de R$ {parcela.Valor:F2} com vencimento em {parcela.DataVencimento:dd/MM/yyyy} referente à ZezaoFutsal. Por favor, regularize o pagamento.";

            bool enviado = await _whatsAppService.EnviarMensagemAsync(telefoneResponsavel, mensagem);
            if (!enviado)
                return new ParcelaResponseDTO { Status = false, Message = "Falha ao enviar a cobrança via WhatsApp." };

            var tipoAviso = parcela.DataVencimento < hoje ? TipoAvisoCobranca.AvisoAtraso : TipoAvisoCobranca.AvisoPrevio;
            var aviso = new AvisoCobranca(parcela.Id, tipoAviso, telefoneResponsavel);
            await _avisoRepository.AdicionarAsync(aviso);

            return new ParcelaResponseDTO { Status = true, Message = "Cobrança disparada via WhatsApp com sucesso." };
        }

        public async Task<ResumoFinanceiroResponseDTO> ObterResumoAsync()
        {
            var parcelas = await _parcelaRepository.ObterTodasAsync();
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

            decimal recebido = parcelas.Where(p => p.Status == StatusParcela.Pago).Sum(p => p.Valor);
            decimal pendente = parcelas.Where(p => p.Status == StatusParcela.Pendente && p.DataVencimento >= hoje).Sum(p => p.Valor);
            decimal inadimplencia = parcelas.Where(p => p.Status == StatusParcela.Pendente && p.DataVencimento < hoje).Sum(p => p.Valor);

            return new ResumoFinanceiroResponseDTO
            {
                Status = true,
                Message = "Resumo financeiro obtido com sucesso.",
                Data = new ResumoFinanceiroData
                {
                    Recebido = recebido,
                    Pendente = pendente,
                    Inadimplencia = inadimplencia
                }
            };
        }
    }
}