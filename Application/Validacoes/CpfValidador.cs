namespace Application.Validacoes;

public static class CpfValidador
{
    private const int QuantidadeDeDigitos = 11;

    public static string Normalizar(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;

        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    public static bool EhValido(string? cpf)
    {
        var digitos = Normalizar(cpf);

        if (digitos.Length != QuantidadeDeDigitos) return false;
        if (digitos.Distinct().Count() == 1) return false;

        var primeiroDigitoVerificador = CalcularDigitoVerificador(digitos, 9);
        if (primeiroDigitoVerificador != digitos[9] - '0') return false;

        var segundoDigitoVerificador = CalcularDigitoVerificador(digitos, 10);
        return segundoDigitoVerificador == digitos[10] - '0';
    }

    public static string Formatar(string? cpf)
    {
        var digitos = Normalizar(cpf);
        if (digitos.Length != QuantidadeDeDigitos) return digitos;

        return $"{digitos[..3]}.{digitos[3..6]}.{digitos[6..9]}-{digitos[9..]}";
    }

    private static int CalcularDigitoVerificador(string digitos, int quantidadeDeDigitosConsiderados)
    {
        var soma = 0;
        for (var posicao = 0; posicao < quantidadeDeDigitosConsiderados; posicao++)
        {
            soma += (digitos[posicao] - '0') * (quantidadeDeDigitosConsiderados + 1 - posicao);
        }

        var resto = (soma * 10) % 11;
        return resto == 10 ? 0 : resto;
    }
}