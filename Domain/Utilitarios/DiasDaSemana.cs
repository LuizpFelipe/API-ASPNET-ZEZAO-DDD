using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Utilitarios;

public static class DiasDaSemana
{
    private const string SufixoFeira = "-feira";

    private static readonly (DayOfWeek Dia, string Nome)[] DiasDeAula =
    {
        (DayOfWeek.Monday, "Segunda"),
        (DayOfWeek.Tuesday, "Terça"),
        (DayOfWeek.Wednesday, "Quarta"),
        (DayOfWeek.Thursday, "Quinta"),
        (DayOfWeek.Friday, "Sexta"),
        (DayOfWeek.Saturday, "Sábado")
    };

    public static IReadOnlyList<DayOfWeek> Converter(string? texto)
    {
        var abreviacoesEncontradas = new HashSet<string>();

        foreach (Match trecho in Regex.Matches(texto ?? string.Empty, @"\p{L}+"))
        {
            var abreviacao = NormalizarTexto(trecho.Value);
            abreviacoesEncontradas.Add(abreviacao.Length > 3 ? abreviacao[..3] : abreviacao);
        }

        return DiasDeAula
            .Where(diaDeAula => abreviacoesEncontradas.Contains(NormalizarTexto(diaDeAula.Nome)[..3]))
            .Select(diaDeAula => diaDeAula.Dia)
            .ToList();
    }

    public static string Formatar(IEnumerable<DayOfWeek> dias)
    {
        var diasInformados = dias.ToHashSet();

        return string.Join(",", DiasDeAula
            .Where(diaDeAula => diasInformados.Contains(diaDeAula.Dia))
            .Select(diaDeAula => diaDeAula.Nome));
    }

    public static string Padronizar(string? texto) => Formatar(Converter(texto));

    public static bool EhValido(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;

        var nomesValidos = DiasDeAula.Select(diaDeAula => NormalizarTexto(diaDeAula.Nome)).ToHashSet();

        foreach (var parte in texto.Split(',', StringSplitOptions.TrimEntries))
        {
            var nomeNormalizado = NormalizarTexto(parte);
            if (nomeNormalizado.EndsWith(SufixoFeira)) nomeNormalizado = nomeNormalizado[..^SufixoFeira.Length];

            if (!nomesValidos.Contains(nomeNormalizado)) return false;
        }

        return true;
    }

    private static string NormalizarTexto(string texto)
    {
        var decomposto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);

        return new string(decomposto
            .Where(caractere => CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            .ToArray());
    }
}