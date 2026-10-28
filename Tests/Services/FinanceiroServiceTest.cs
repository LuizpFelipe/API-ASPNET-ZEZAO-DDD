using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Tests
{
    public class FinanceiroServiceTest
    {
        private static readonly DateOnly Hoje = new DateOnly(2026, 10, 20);

        private readonly IParcelaRepository _parcelaRepository = Substitute.For<IParcelaRepository>();
        private readonly IAvisoCobrancaRepository _avisoRepository = Substitute.For<IAvisoCobrancaRepository>();
        private readonly IWhatsAppService _whatsAppService = Substitute.For<IWhatsAppService>();
        private readonly IRelogio _relogio = Substitute.For<IRelogio>();
        private readonly FinanceiroService _service;

        public FinanceiroServiceTest()
        {
            _relogio.HojeNoBrasil().Returns(Hoje);
            _service = new FinanceiroService(_parcelaRepository, _avisoRepository, _whatsAppService, _relogio);
        }

        private static ParcelaDetalhada CriarDetalhada(
            Parcela parcela,
            string telefone = "(44) 99999-1234",
            StatusContrato statusContrato = StatusContrato.Ativo)
        {
            return new ParcelaDetalhada(parcela, Guid.NewGuid(), "Enzo Ferreira", "Marcos Ferreira", telefone, statusContrato);
        }

        private void MockarDetalhadas(params ParcelaDetalhada[] itens)
        {
            IEnumerable<ParcelaDetalhada> retorno = new List<ParcelaDetalhada>(itens);

            _parcelaRepository
                .ObterDetalhadasAsync(Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<DateOnly>())
                .Returns(retorno);
        }

        [Fact]
        public async Task RegistrarPagamento_Should_Mark_As_Paid_With_Brazil_Date()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(5), 150.00m);
            _parcelaRepository.ObterPorIdAsync(parcela.Id).Returns(parcela);

            var result = await _service.RegistrarPagamentoAsync(parcela.Id);

            Assert.True(result.Status);
            Assert.Equal("Pagamento registrado com sucesso.", result.Message);
            Assert.Equal(StatusParcela.Pago, parcela.Status);
            Assert.Equal(Hoje, parcela.DataPagamento);
            await _parcelaRepository.Received(1).AtualizarAsync(parcela);
        }

        [Fact]
        public async Task RegistrarPagamento_Should_Not_Return_The_Entity_In_Data()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(5), 150.00m);
            _parcelaRepository.ObterPorIdAsync(parcela.Id).Returns(parcela);

            var result = await _service.RegistrarPagamentoAsync(parcela.Id);

            Assert.NotNull(result.Data);
            Assert.IsNotType<Parcela>(result.Data);

            var dados = JsonSerializer.SerializeToElement(result.Data);
            Assert.Equal("Pago", dados.GetProperty("Status").GetString());
            Assert.Equal(parcela.Id, dados.GetProperty("Id").GetGuid());
        }

        [Fact]
        public async Task RegistrarPagamento_Should_Return_Error_When_Parcela_Not_Found()
        {
            var parcelaId = Guid.NewGuid();
            _parcelaRepository.ObterPorIdAsync(parcelaId).Returns((Parcela?)null);

            var result = await _service.RegistrarPagamentoAsync(parcelaId);

            Assert.False(result.Status);
            Assert.Equal("Parcela não encontrada.", result.Message);
        }

        [Fact]
        public async Task RegistrarPagamento_Should_Return_Error_When_Already_Paid()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(-5), 200.00m);
            parcela.MarcarComoPaga(Hoje);
            _parcelaRepository.ObterPorIdAsync(parcela.Id).Returns(parcela);

            var result = await _service.RegistrarPagamentoAsync(parcela.Id);

            Assert.False(result.Status);
            Assert.Equal("Esta parcela já se encontra paga.", result.Message);
            await _parcelaRepository.DidNotReceive().AtualizarAsync(Arg.Any<Parcela>());
        }

        [Fact]
        public async Task DispararCobranca_Should_Send_To_Responsavel_Phone_And_Save_Aviso()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(5), 300.00m);
            _parcelaRepository.ObterDetalhadaPorIdAsync(parcela.Id).Returns(CriarDetalhada(parcela));
            _whatsAppService.EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var result = await _service.DispararCobrancaAsync(parcela.Id);

            Assert.True(result.Status);
            Assert.Equal("Cobrança disparada via WhatsApp com sucesso.", result.Message);
            await _whatsAppService.Received(1).EnviarMensagemAsync(
                "5544999991234",
                Arg.Is<string>(m => m.Contains("Enzo Ferreira") && m.Contains("parcela 1")));
            await _avisoRepository.Received(1).AdicionarAsync(
                Arg.Is<AvisoCobranca>(a => a.Destinatario == "5544999991234" && a.Tipo == TipoAvisoCobranca.AvisoPrevio));
        }

        [Fact]
        public async Task DispararCobranca_Should_Use_Aviso_Atraso_When_Parcela_Is_Overdue()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(-3), 300.00m);
            _parcelaRepository.ObterDetalhadaPorIdAsync(parcela.Id).Returns(CriarDetalhada(parcela));
            _whatsAppService.EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var result = await _service.DispararCobrancaAsync(parcela.Id);

            Assert.True(result.Status);
            await _avisoRepository.Received(1).AdicionarAsync(Arg.Is<AvisoCobranca>(a => a.Tipo == TipoAvisoCobranca.AvisoAtraso));
        }

        [Fact]
        public async Task DispararCobranca_Should_Return_Error_When_Parcela_Not_Found()
        {
            var parcelaId = Guid.NewGuid();
            _parcelaRepository.ObterDetalhadaPorIdAsync(parcelaId).Returns((ParcelaDetalhada?)null);

            var result = await _service.DispararCobrancaAsync(parcelaId);

            Assert.False(result.Status);
            Assert.Equal("Parcela não encontrada.", result.Message);
        }

        [Fact]
        public async Task DispararCobranca_Should_Return_Error_When_Parcela_Already_Paid()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(-5), 300.00m);
            parcela.MarcarComoPaga(Hoje);
            _parcelaRepository.ObterDetalhadaPorIdAsync(parcela.Id).Returns(CriarDetalhada(parcela));

            var result = await _service.DispararCobrancaAsync(parcela.Id);

            Assert.False(result.Status);
            Assert.Equal("Esta parcela já foi paga. Não há cobrança para enviar.", result.Message);
            await _whatsAppService.DidNotReceive().EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>());
            await _avisoRepository.DidNotReceive().AdicionarAsync(Arg.Any<AvisoCobranca>());
        }

        [Fact]
        public async Task DispararCobranca_Should_Return_Error_When_Contract_Is_Canceled()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(-5), 300.00m);
            _parcelaRepository.ObterDetalhadaPorIdAsync(parcela.Id)
                .Returns(CriarDetalhada(parcela, statusContrato: StatusContrato.Cancelado));

            var result = await _service.DispararCobrancaAsync(parcela.Id);

            Assert.False(result.Status);
            await _whatsAppService.DidNotReceive().EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DispararCobranca_Should_Return_Error_When_Phone_Is_Invalid()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(5), 300.00m);
            _parcelaRepository.ObterDetalhadaPorIdAsync(parcela.Id).Returns(CriarDetalhada(parcela, telefone: "123"));

            var result = await _service.DispararCobrancaAsync(parcela.Id);

            Assert.False(result.Status);
            Assert.Equal("O responsável deste aluno não tem um telefone válido cadastrado.", result.Message);
            await _whatsAppService.DidNotReceive().EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DispararCobranca_Should_Return_Error_When_WhatsApp_Fails()
        {
            var parcela = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(5), 300.00m);
            _parcelaRepository.ObterDetalhadaPorIdAsync(parcela.Id).Returns(CriarDetalhada(parcela));
            _whatsAppService.EnviarMensagemAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var result = await _service.DispararCobrancaAsync(parcela.Id);

            Assert.False(result.Status);
            Assert.Equal("Falha ao enviar a cobrança via WhatsApp.", result.Message);
            await _avisoRepository.DidNotReceive().AdicionarAsync(Arg.Any<AvisoCobranca>());
        }

        [Fact]
        public async Task ListarParcelas_Should_Include_Aluno_And_Calculate_Atrasado()
        {
            var parcela = new Parcela(Guid.NewGuid(), 2, Hoje.AddDays(-4), 150.00m);
            var detalhada = CriarDetalhada(parcela);
            MockarDetalhadas(detalhada);

            var result = await _service.ListarParcelasAsync(null, null, null);

            var item = JsonSerializer.SerializeToElement(Assert.Single(result));
            Assert.Equal(detalhada.AlunoId, item.GetProperty("AlunoId").GetGuid());
            Assert.Equal("Enzo Ferreira", item.GetProperty("Aluno").GetString());
            Assert.Equal(2, item.GetProperty("Numero").GetInt32());
            Assert.Equal(150.00m, item.GetProperty("Valor").GetDecimal());
            Assert.Equal("Atrasado", item.GetProperty("Status").GetString());
        }

        [Fact]
        public async Task ListarParcelas_Should_Return_Pendente_When_Due_Today_Or_Later()
        {
            var parcela = new Parcela(Guid.NewGuid(), 3, Hoje, 150.00m);
            MockarDetalhadas(CriarDetalhada(parcela));

            var result = await _service.ListarParcelasAsync(null, null, null);

            var item = JsonSerializer.SerializeToElement(Assert.Single(result));
            Assert.Equal("Pendente", item.GetProperty("Status").GetString());
        }

        [Fact]
        public async Task ListarParcelas_Should_Pass_Filters_And_Brazil_Date_To_Repository()
        {
            var alunoId = Guid.NewGuid();
            MockarDetalhadas();

            await _service.ListarParcelasAsync(alunoId, "Atrasado", 10);

            await _parcelaRepository.Received(1).ObterDetalhadasAsync(alunoId, "Atrasado", 10, Hoje);
        }

        [Fact]
        public async Task ObterResumo_Should_Sum_Only_Current_Month_As_Received()
        {
            var paga = new Parcela(Guid.NewGuid(), 1, Hoje.AddDays(-15), 100.00m);
            paga.MarcarComoPaga(Hoje.AddDays(-10));

            var pagaNoMesPassado = new Parcela(Guid.NewGuid(), 1, new DateOnly(2026, 9, 5), 80.00m);
            pagaNoMesPassado.MarcarComoPaga(new DateOnly(2026, 9, 20));

            var pendente = new Parcela(Guid.NewGuid(), 2, Hoje.AddDays(15), 250.00m);
            var atrasada = new Parcela(Guid.NewGuid(), 3, Hoje.AddDays(-5), 150.00m);

            MockarDetalhadas(
                CriarDetalhada(paga),
                CriarDetalhada(pagaNoMesPassado),
                CriarDetalhada(pendente),
                CriarDetalhada(atrasada));

            var result = await _service.ObterResumoAsync();

            Assert.True(result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(100.00m, result.Data.RecebidoMes);
            Assert.Equal(250.00m, result.Data.Pendente);
            Assert.Equal(150.00m, result.Data.Inadimplencia);
        }

        [Fact]
        public async Task ObterResumo_Should_Use_Brazil_Date_To_Split_Pendente_And_Inadimplencia()
        {
            var venceHoje = new Parcela(Guid.NewGuid(), 1, Hoje, 120.00m);
            var venceuOntem = new Parcela(Guid.NewGuid(), 2, Hoje.AddDays(-1), 130.00m);

            MockarDetalhadas(CriarDetalhada(venceHoje), CriarDetalhada(venceuOntem));

            var result = await _service.ObterResumoAsync();

            Assert.Equal(120.00m, result.Data!.Pendente);
            Assert.Equal(130.00m, result.Data.Inadimplencia);
            Assert.Equal(0m, result.Data.RecebidoMes);
        }
    }
}