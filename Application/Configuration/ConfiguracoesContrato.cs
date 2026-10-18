namespace Application.Configuration;

public class ConfiguracoesContrato
{
    public decimal ValorParcela { get; set; }
    public int DiaVencimento { get; set; }
    public int QuantidadeParcelas { get; set; }
    public string NomeEscola { get; set; } = string.Empty;
    public string DocumentoEscola { get; set; } = string.Empty;
    public string NomeResponsavelLegal { get; set; } = string.Empty;
}
