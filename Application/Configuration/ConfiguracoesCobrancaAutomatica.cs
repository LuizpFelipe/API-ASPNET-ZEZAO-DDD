namespace Application.Configuration;

public class ConfiguracoesCobrancaAutomatica
{
    public bool Habilitada { get; set; } = true;

    /// <summary>Quantos dias antes do vencimento o responsável recebe o aviso prévio.</summary>
    public int DiasAntesDoVencimento { get; set; } = 10;

    /// <summary>Quantos dias depois do vencimento, sem pagamento, o responsável é avisado de novo e a equipe é notificada.</summary>
    public int DiasAposVencimentoParaReavisar { get; set; } = 10;

    /// <summary>De quanto em quanto tempo a rotina roda. Ela é idempotente: não repete avisos já enviados.</summary>
    public int IntervaloEmMinutos { get; set; } = 60;
}
