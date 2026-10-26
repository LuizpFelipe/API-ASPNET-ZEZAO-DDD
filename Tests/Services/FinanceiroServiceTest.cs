using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Services;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Tests
{
    public class FinanceiroServiceTest
    {
        private readonly IParcelaRepository _parcelaRepository = Substitute.For<IParcelaRepository>();
        private readonly IAvisoCobrancaRepository _avisoRepository = Substitute.For<IAvisoCobrancaRepository>();
        private readonly IWhatsAppService _whatsAppService = Substitute.For<IWhatsAppService>();
        private readonly FinanceiroService _service;

        public FinanceiroServiceTest()
        {
            _service = new FinanceiroService(_parcelaRepository, _avisoRepository, _whatsAppService);
        }

        [Fact]
        public async Task RegistrarPagamento_Should_Mark_As_Paid_Correctly()
        {
            // Arrange
            var parcelaId = Guid.NewGuid();
            var contratoId = Guid.NewGuid();
            var parcela = new Parcela(contratoId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 150.00m);

            _parcelaRepository.ObterPorIdAsync(parcelaId).Returns(parcela);

            // Act
            var result = await _service.RegistrarPagamentoAsync(parcelaId);

            // Assert
            Assert.True(result.Status);
            Assert.Equal("Pagamento registrado com sucesso.", result.Message);
            Assert.Equal(StatusParcela.Pago, parcela.Status);
            await _parcelaRepository.Received(1).AtualizarAsync(parcela);
        }

        [Fact]
        public async Task RegistrarPagamento_Should_Return_Error_When_Parcela_Not_Found()
        {
            // Arrange
            var parcelaId = Guid.NewGuid();
            _parcelaRepository.ObterPorIdAsync(parcelaId).Returns((Parcela?)null);

            // Act
            var result = await _service.RegistrarPagamentoAsync(parcelaId);

            // Assert
            Assert.False(result.Status);
            Assert.Equal("Parcela não encontrada.", result.Message);
        }

        [Fact]
        public async Task RegistrarPagamento_Should_Return_Error_When_Already_Paid()
        {
            // Arrange
            var parcelaId = Guid.NewGuid();
            var contratoId = Guid.NewGuid();
            var parcela = new Parcela(contratoId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), 200.00m);
            parcela.MarcarComoPaga(DateOnly.FromDateTime(DateTime.UtcNow));

            _parcelaRepository.ObterPorIdAsync(parcelaId).Returns(parcela);

            // Act
            var result = await _service.RegistrarPagamentoAsync(parcelaId);

            // Assert
            Assert.False(result.Status);
            Assert.Equal("Esta parcela já se encontra paga.", result.Message);
        }

        [Fact]
        public async Task DispararCobranca_Should_Send_WhatsApp_And_Save_Aviso()
        {
            // Arrange
            var parcelaId = Guid.NewGuid();
            var contratoId = Guid.NewGuid();
            var parcela = new Parcela(contratoId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 300.00m);

            _parcelaRepository.ObterPorIdAsync(parcelaId).Returns(parcela);
            _whatsAppService.EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            // Act
            var result = await _service.DispararCobrancaAsync(parcelaId);

            // Assert
            Assert.True(result.Status);
            Assert.Equal("Cobrança disparada via WhatsApp com sucesso.", result.Message);
            await _whatsAppService.Received(1).EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>());
            await _avisoRepository.Received(1).AdicionarAsync(Arg.Any<AvisoCobranca>());
        }

        [Fact]
        public async Task DispararCobranca_Should_Return_Error_When_WhatsApp_Fails()
        {
            // Arrange
            var parcelaId = Guid.NewGuid();
            var contratoId = Guid.NewGuid();
            var parcela = new Parcela(contratoId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 300.00m);

            _parcelaRepository.ObterPorIdAsync(parcelaId).Returns(parcela);
            _whatsAppService.EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            // Act
            var result = await _service.DispararCobrancaAsync(parcelaId);

            // Assert
            Assert.False(result.Status);
            Assert.Equal("Falha ao enviar a cobrança via WhatsApp.", result.Message);
            await _avisoRepository.DidNotReceive().AdicionarAsync(Arg.Any<AvisoCobranca>());
        }

        [Fact]
        public async Task ObterResumo_Should_Calculate_Financial_Summary_Correctly()
        {
            // Arrange
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

            var parcelaPaga = new Parcela(Guid.NewGuid(), 1, hoje.AddDays(-10), 100.00m);
            parcelaPaga.MarcarComoPaga(hoje.AddDays(-5));

            var parcelaPendente = new Parcela(Guid.NewGuid(), 2, hoje.AddDays(10), 250.00m);
            var parcelaAtrasada = new Parcela(Guid.NewGuid(), 3, hoje.AddDays(-5), 150.00m);

            var parcelas = new List<Parcela> { parcelaPaga, parcelaPendente, parcelaAtrasada };
            _parcelaRepository.ObterTodasAsync().Returns(parcelas);

            // Act
            var result = await _service.ObterResumoAsync();

            // Assert
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(100.00m, result.Data.Recebido);
            Assert.Equal(250.00m, result.Data.Pendente);
            Assert.Equal(150.00m, result.Data.Inadimplencia);
        }
    }
}