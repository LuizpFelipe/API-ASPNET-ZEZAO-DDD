using Domain.Entities;

namespace Application.Campeonatos
{
    public static class PesosDoRanking
    {
        public const decimal Gol = 5m;
        public const decimal Assistencia = 3m;
        public const decimal PasseCerto = 0.1m;
        public const decimal Desarme = 1m;
        public const decimal Defesa = 1m;
        public const decimal Falta = -1m;
        public const decimal CartaoAmarelo = -2m;
        public const decimal CartaoVermelho = -5m;
    }

    public record LinhaDoRanking(
        Guid AlunoId, int Jogos, int Gols, int Assistencias, int PassesCertos, int Desarmes,
        int Defesas, int Faltas, int CartoesAmarelos, int CartoesVermelhos, decimal Pontos);

    public static class CalculadoraDeRanking
    {
        public static List<LinhaDoRanking> Calcular(IEnumerable<EstatisticaJogo> estatisticas, IEnumerable<Guid> alunosParticipantes)
        {
            var porAluno = estatisticas.GroupBy(estatistica => estatistica.AlunoId).ToDictionary(grupo => grupo.Key, grupo => grupo.ToList());

            var linhas = new List<LinhaDoRanking>();
            foreach (var alunoId in alunosParticipantes.Distinct())
            {
                porAluno.TryGetValue(alunoId, out var registros);
                registros ??= new List<EstatisticaJogo>();

                var gols = registros.Sum(registro => registro.Gols);
                var assistencias = registros.Sum(registro => registro.Assistencias);
                var passesCertos = registros.Sum(registro => registro.PassesCertos);
                var desarmes = registros.Sum(registro => registro.Desarmes);
                var defesas = registros.Sum(registro => registro.Defesas);
                var faltas = registros.Sum(registro => registro.Faltas);
                var amarelos = registros.Sum(registro => registro.CartoesAmarelos);
                var vermelhos = registros.Sum(registro => registro.CartoesVermelhos);

                var pontos = gols * PesosDoRanking.Gol
                           + assistencias * PesosDoRanking.Assistencia
                           + passesCertos * PesosDoRanking.PasseCerto
                           + desarmes * PesosDoRanking.Desarme
                           + defesas * PesosDoRanking.Defesa
                           + faltas * PesosDoRanking.Falta
                           + amarelos * PesosDoRanking.CartaoAmarelo
                           + vermelhos * PesosDoRanking.CartaoVermelho;

                linhas.Add(new LinhaDoRanking(alunoId, registros.Select(registro => registro.JogoId).Distinct().Count(),
                    gols, assistencias, passesCertos, desarmes, defesas, faltas, amarelos, vermelhos, Math.Round(pontos, 1)));
            }

            return linhas
                .OrderByDescending(linha => linha.Pontos)
                .ThenByDescending(linha => linha.Gols)
                .ThenByDescending(linha => linha.Assistencias)
                .ThenBy(linha => linha.AlunoId)
                .ToList();
        }
    }
}
