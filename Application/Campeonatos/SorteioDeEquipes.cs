using Domain.Enums;

namespace Application.Campeonatos
{
    public record CandidatoAoSorteio(Guid AlunoId, PosicaoJogador Posicao);

    public record ResultadoDoSorteio(List<List<Guid>> Equipes, List<Guid> Sobras, int MaximoDeEquipes, string? Erro);

    public static class SorteioDeEquipes
    {
        public static ResultadoDoSorteio Sortear(IEnumerable<CandidatoAoSorteio> candidatos, int? quantidadeDesejada, Random aleatorio)
        {
            var candidatosPorPosicao = NomesDePosicao.TodasAsPosicoes.ToDictionary(
                posicao => posicao,
                posicao => candidatos.Where(candidato => candidato.Posicao == posicao).Select(candidato => candidato.AlunoId).ToList());

            var maximoDeEquipes = candidatosPorPosicao.Values.Min(lista => lista.Count);
            var quantidade = quantidadeDesejada ?? maximoDeEquipes;

            if (quantidade < 1)
            {
                var posicoesSemAluno = ListarPosicoesComMenosDe(candidatosPorPosicao, 1);
                var detalhe = posicoesSemAluno.Count > 0
                    ? $" Faltam alunos nas posições: {string.Join(", ", posicoesSemAluno)}."
                    : string.Empty;
                return new ResultadoDoSorteio(new(), new(), maximoDeEquipes, $"Não há alunos suficientes para formar um time completo.{detalhe}");
            }

            if (quantidade > maximoDeEquipes)
            {
                var posicoesInsuficientes = ListarPosicoesComMenosDe(candidatosPorPosicao, quantidade);
                return new ResultadoDoSorteio(new(), new(), maximoDeEquipes,
                    $"Só é possível formar {maximoDeEquipes} time(s) com os alunos disponíveis. Faltam alunos nas posições: {string.Join(", ", posicoesInsuficientes)}.");
            }

            var embaralhados = candidatosPorPosicao.ToDictionary(par => par.Key, par => Embaralhar(par.Value, aleatorio));

            var equipes = new List<List<Guid>>();
            for (var indice = 0; indice < quantidade; indice++)
            {
                equipes.Add(NomesDePosicao.TodasAsPosicoes.Select(posicao => embaralhados[posicao][indice]).ToList());
            }

            var sobras = embaralhados.Values.SelectMany(lista => lista.Skip(quantidade)).ToList();
            return new ResultadoDoSorteio(equipes, sobras, maximoDeEquipes, null);
        }

        private static List<string> ListarPosicoesComMenosDe(Dictionary<PosicaoJogador, List<Guid>> candidatosPorPosicao, int minimo)
        {
            return candidatosPorPosicao
                .Where(par => par.Value.Count < minimo)
                .Select(par => NomesDePosicao.Obter(par.Key))
                .ToList();
        }

        private static List<Guid> Embaralhar(List<Guid> origem, Random aleatorio)
        {
            var copia = origem.ToList();
            for (var indice = copia.Count - 1; indice > 0; indice--)
            {
                var sorteado = aleatorio.Next(indice + 1);
                (copia[indice], copia[sorteado]) = (copia[sorteado], copia[indice]);
            }
            return copia;
        }
    }
}
