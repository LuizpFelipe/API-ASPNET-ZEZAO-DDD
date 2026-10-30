using Application.Configuration;
using Application.Interfaces;

namespace Api.Servicos;

/// <summary>
/// Roda as rotinas automáticas em intervalos fixos: promoção de categoria por aniversário e rotina financeira
/// (status de inadimplência + avisos de cobrança). As duas são idempotentes, então rodar várias vezes ao dia não repete nada.
/// </summary>
public class RotinasDiariasEmSegundoPlano : BackgroundService
{
    private static readonly TimeSpan AtrasoInicial = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _fabricaDeEscopos;
    private readonly ConfiguracoesCobrancaAutomatica _configuracoes;
    private readonly ILogger<RotinasDiariasEmSegundoPlano> _logger;

    public RotinasDiariasEmSegundoPlano(
        IServiceScopeFactory fabricaDeEscopos,
        ConfiguracoesCobrancaAutomatica configuracoes,
        ILogger<RotinasDiariasEmSegundoPlano> logger)
    {
        _fabricaDeEscopos = fabricaDeEscopos;
        _configuracoes = configuracoes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken tokenDeParada)
    {
        var intervalo = TimeSpan.FromMinutes(Math.Max(1, _configuracoes.IntervaloEmMinutos));

        try
        {
            await Task.Delay(AtrasoInicial, tokenDeParada);

            while (!tokenDeParada.IsCancellationRequested)
            {
                await ExecutarUmaVezAsync();
                await Task.Delay(intervalo, tokenDeParada);
            }
        }
        catch (OperationCanceledException)
        {
            // Aplicação encerrando.
        }
    }

    private async Task ExecutarUmaVezAsync()
    {
        await ExecutarPromocaoDeCategoriaAsync();
        await ExecutarRotinaFinanceiraAsync();
    }

    private async Task ExecutarPromocaoDeCategoriaAsync()
    {
        try
        {
            using var escopo = _fabricaDeEscopos.CreateScope();
            var promocao = escopo.ServiceProvider.GetRequiredService<IPromocaoDeCategoriaService>();
            var resultado = await promocao.ExecutarAsync();

            _logger.LogInformation("Promoção de categoria: {Promovidos} aluno(s) promovido(s).", resultado.AlunosPromovidos);

            foreach (var aluno in resultado.AlunosSemTurmaNaNovaCategoria)
            {
                _logger.LogWarning("Aluno deveria subir de categoria, mas não há turma cadastrada na nova categoria: {Aluno}", aluno);
            }
        }
        catch (Exception excecao)
        {
            _logger.LogError(excecao, "Falha ao executar a promoção de categoria. Ela será tentada de novo no próximo ciclo.");
        }
    }

    private async Task ExecutarRotinaFinanceiraAsync()
    {
        try
        {
            using var escopo = _fabricaDeEscopos.CreateScope();
            var rotina = escopo.ServiceProvider.GetRequiredService<IRotinaFinanceiraService>();
            var resultado = await rotina.ExecutarAsync();

            _logger.LogInformation(
                "Rotina financeira: {Marcados} aluno(s) marcados como inadimplentes, {Regularizados} regularizados, " +
                "{Previos} aviso(s) prévio(s), {Atrasos} aviso(s) de atraso, {Falhas} falha(s) de envio.",
                resultado.AlunosMarcadosComoInadimplentes, resultado.AlunosRegularizados,
                resultado.AvisosPreviosEnviados, resultado.AvisosDeAtrasoEnviados, resultado.FalhasDeEnvio);
        }
        catch (Exception excecao)
        {
            _logger.LogError(excecao, "Falha ao executar a rotina financeira. Ela será tentada de novo no próximo ciclo.");
        }
    }
}
