using Domain.Entities;
using Domain.Enums;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Mantém os dados de demonstração coerentes com a data atual: completa as chamadas dos dias que passaram
    /// e dá baixa nas parcelas que venceram, deixando em atraso só os alunos inadimplentes "por design".
    /// </summary>
    public static class AtualizacaoDeDadosDeDemonstracao
    {
        private const int PercentualDeAlunosInadimplentesPorDesign = 25;
        private const int LimiteDeDiasDeChamadaRetroativa = 120;
        private static readonly double[] TaxasDePresenca = { 0.35, 0.50, 0.70, 0.90, 0.97 };

        public readonly record struct ResultadoDaAtualizacao(
            int PresencasCriadas,
            int ParcelasPagas,
            int AlunosRegularizados)
        {
            public bool HouveMudancas => PresencasCriadas > 0 || ParcelasPagas > 0 || AlunosRegularizados > 0;
        }

        public static async Task<ResultadoDaAtualizacao> AtualizarAteAsync(
            AppDbContext contexto,
            DateOnly dataDeReferencia,
            bool deixarUltimaAulaSemChamada = true)
        {
            var idsDosAlunosDeDemonstracao = (await contexto.Set<Presenca>()
                    .Select(presenca => presenca.AlunoId)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet();

            var alunosAtivos = (await contexto.Set<Aluno>()
                    .Where(aluno => aluno.Status != StatusAluno.Inativo)
                    .ToListAsync())
                .Where(aluno => idsDosAlunosDeDemonstracao.Contains(aluno.Id))
                .ToList();

            var presencasCriadas = await CompletarChamadasAsync(contexto, alunosAtivos, dataDeReferencia, deixarUltimaAulaSemChamada);
            var (parcelasPagas, alunosRegularizados) = await AtualizarParcelasAsync(contexto, alunosAtivos, dataDeReferencia);

            await contexto.SaveChangesAsync();

            return new ResultadoDaAtualizacao(presencasCriadas, parcelasPagas, alunosRegularizados);
        }

        private static async Task<int> CompletarChamadasAsync(
            AppDbContext contexto,
            List<Aluno> alunosAtivos,
            DateOnly dataDeReferencia,
            bool deixarUltimaAulaSemChamada)
        {
            var turmasAtivas = (await contexto.Set<Turma>().Where(turma => turma.Ativa).ToListAsync())
                .OrderBy(turma => turma.Id)
                .ToList();

            var ultimaChamadaPorTurma = await contexto.Set<Presenca>()
                .GroupBy(presenca => presenca.TurmaId)
                .Select(grupo => new { TurmaId = grupo.Key, UltimaData = grupo.Max(presenca => presenca.Data) })
                .ToDictionaryAsync(item => item.TurmaId, item => item.UltimaData);

            var limiteInferior = dataDeReferencia.AddDays(-LimiteDeDiasDeChamadaRetroativa);
            var chavesJaExistentes = (await contexto.Set<Presenca>()
                    .Where(presenca => presenca.Data >= limiteInferior)
                    .Select(presenca => new { presenca.AlunoId, presenca.TurmaId, presenca.Data })
                    .ToListAsync())
                .Select(item => (item.AlunoId, item.TurmaId, item.Data))
                .ToHashSet();

            var presencasCriadas = 0;

            for (var indiceDaTurma = 0; indiceDaTurma < turmasAtivas.Count; indiceDaTurma++)
            {
                var turma = turmasAtivas[indiceDaTurma];
                if (!ultimaChamadaPorTurma.TryGetValue(turma.Id, out var ultimaChamada)) continue;

                var primeiraData = ultimaChamada.AddDays(1);
                if (primeiraData < limiteInferior) primeiraData = limiteInferior;

                var datasDeAula = new List<DateOnly>();
                for (var data = primeiraData; data < dataDeReferencia; data = data.AddDays(1))
                {
                    if (turma.TemAulaEm(data)) datasDeAula.Add(data);
                }

                // Nas turmas de índice par, a aula mais recente fica sem chamada para testar o lançamento.
                if (deixarUltimaAulaSemChamada && indiceDaTurma % 2 == 0 && datasDeAula.Count > 0)
                {
                    datasDeAula.RemoveAt(datasDeAula.Count - 1);
                }

                var alunosDaTurma = alunosAtivos.Where(aluno => aluno.TurmaId == turma.Id).ToList();

                foreach (var data in datasDeAula)
                {
                    foreach (var aluno in alunosDaTurma)
                    {
                        if (data < DateOnly.FromDateTime(aluno.DataCadastro)) continue;
                        if (chavesJaExistentes.Contains((aluno.Id, turma.Id, data))) continue;

                        var sorteio = new Random(unchecked(data.DayNumber * 397 ^ BitConverter.ToInt32(aluno.Id.ToByteArray(), 0)));
                        var taxaDePresenca = TaxasDePresenca[(int)(BitConverter.ToUInt32(aluno.Id.ToByteArray(), 0) % (uint)TaxasDePresenca.Length)];

                        contexto.Set<Presenca>().Add(new Presenca(aluno.Id, turma.Id, data, sorteio.NextDouble() < taxaDePresenca));
                        presencasCriadas++;
                    }
                }
            }

            return presencasCriadas;
        }

        private static async Task<(int ParcelasPagas, int AlunosRegularizados)> AtualizarParcelasAsync(
            AppDbContext contexto,
            List<Aluno> alunosAtivos,
            DateOnly dataDeReferencia)
        {
            var alunoPorId = alunosAtivos.ToDictionary(aluno => aluno.Id);
            var idsDosAlunos = alunoPorId.Keys.ToList();

            var contratosAtivos = await contexto.Set<Contrato>()
                .Where(contrato => contrato.Status == StatusContrato.Ativo && idsDosAlunos.Contains(contrato.AlunoId))
                .ToListAsync();

            var alunoPorContrato = contratosAtivos.ToDictionary(contrato => contrato.Id, contrato => contrato.AlunoId);
            var idsDosContratos = alunoPorContrato.Keys.ToList();

            var parcelasVencidasEmAberto = await contexto.Set<Parcela>()
                .Where(parcela => idsDosContratos.Contains(parcela.ContratoId)
                                  && parcela.Status == StatusParcela.Pendente
                                  && parcela.DataVencimento < dataDeReferencia)
                .ToListAsync();

            var parcelasPagas = 0;
            var alunosRegularizados = 0;

            foreach (var grupo in parcelasVencidasEmAberto.GroupBy(parcela => alunoPorContrato[parcela.ContratoId]))
            {
                var aluno = alunoPorId[grupo.Key];

                // Quem é inadimplente "por design" fica com as parcelas em aberto; a rotina financeira cuida do status.
                if (EhInadimplentePorDesign(aluno.Id)) continue;

                foreach (var parcela in grupo)
                {
                    parcela.MarcarComoPaga(parcela.DataVencimento.AddDays(-(parcela.Numero % 4)));
                    parcelasPagas++;
                }

                if (aluno.RegularizarSituacao()) alunosRegularizados++;
            }

            return (parcelasPagas, alunosRegularizados);
        }

        /// <summary>Define de forma estável (pelo Id) quais alunos de demonstração ficam inadimplentes.</summary>
        private static bool EhInadimplentePorDesign(Guid alunoId)
        {
            return BitConverter.ToUInt32(alunoId.ToByteArray(), 4) % 100 < PercentualDeAlunosInadimplentesPorDesign;
        }
    }
}
