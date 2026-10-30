using Application.Configuration;
using Application.Interfaces;

namespace Api.Servicos;

/// <summary>
/// Roda a rotina financeira (status de inadimplência + avisos automáticos de cobrança) em intervalos fixos.
/// A rotina é idempotente, então rodar várias vezes ao dia não duplica avisos.
/// </summary>
public class RotinaFinanceiraEmSegundoPlano : BackgroundService
{
    private static readonly TimeSpan AtrasoInicial = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _fabricaDeEscopos;
    private readonly ConfiguracoesCobrancaAutomatica _configuracoes;
    private readonly ILogger<RotinaFinanceiraEmSegundoPlano> _logger;

    public RotinaFinanceiraEmSegundoPlano(
        IServiceScopeFactory fabricaDeEscopos,
        ConfiguracoesCobrancaAutomatica configuracoes,
        ILogger<RotinaFinanceiraEmSegundoPlano> logger)
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
