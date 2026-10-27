using System.Text.RegularExpressions;

namespace Application.Validacoes
{
    public static class HorarioValidador
    {
        private static readonly Regex PadraoDeHorario = new(
            @"^\s*([01]?\d|2[0-3])\s*(?:h|:)\s*([0-5]\d)?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool EhValido(string? horario)
        {
            return !string.IsNullOrWhiteSpace(horario) && PadraoDeHorario.IsMatch(horario);
        }
    }
}
