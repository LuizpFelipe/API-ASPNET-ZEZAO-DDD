using Domain.Enums;

namespace Application.Campeonatos
{
    public record CandidatoAoSorteio(Guid AlunoId, PosicaoJogador Posicao);

    public record ResultadoDoSorteio(List<List<Guid>> Equipes, List<Guid> Sobras, int MaximoDeEquipes, string? Erro);

    public static class SorteioDeEquipes
    {
        private const int JogadoresMinimosPorEquipe = 5;

        // Modo livre: todos os candidatos entram em algum time. Cada time tenta ter uma posição de cada;
        // posições que faltam são preenchidas por jogadores de outras posições e o excedente vira reserva.
        public static ResultadoDoSorteio SortearTodos(IEnumerable<CandidatoAoSorteio> candidatos, int? quantidadeDesejada, Random aleatorio)
        {
            var lista = candidatos.ToList();
            var maximoDeEquipes = lista.Count / JogadoresMinimosPorEquipe;
            if (maximoDeEquipes < 1)
            {
                return new ResultadoDoSorteio(new(), new(), 0, $"São necessários ao menos {JogadoresMinimosPorEquipe} alunos sem time para formar um time.");
            }

            var quantidade = quantidadeDesejada ?? maximoDeEquipes;
            if (quantidade < 1 || quantidade > maximoDeEquipes)
            {
                return new ResultadoDoSorteio(new(), new(), maximoDeEquipes,
                    $"Com {lista.Count} alunos é possível formar de 1 a {maximoDeEquipes} time(s) de pelo menos {JogadoresMinimosPorEquipe} jogadores.");
            }

            var posicaoDe = lista.ToDictionary(candidato => candidato.AlunoId, candidato => candidato.Posicao);
            var porPosicao = NomesDePosicao.TodasAsPosicoes.ToDictionary(
                posicao => posicao,
                posicao => Embaralhar(lista.Where(candidato => candidato.Posicao == posicao).Select(candidato => candidato.AlunoId).ToList(), aleatorio));

            var equipes = Enumerable.Range(0, quantidade).Select(_ => new List<Guid>()).ToList();
            var restantes = new List<Guid>();

            foreach (var posicao in NomesDePosicao.TodasAsPosicoes)
            {
                var jogadores = porPosicao[posicao];
                for (var indice = 0; indice < jogadores.Count; indice++)
                {
                    if (indice < quantidade) equipes[indice].Add(jogadores[indice]);
                    else restantes.Add(jogadores[indice]);
                }
            }

            restantes = Embaralhar(restantes, aleatorio);

            foreach (var equipe in equipes.OrderBy(equipe => equipe.Count))
            {
                while (equipe.Count < JogadoresMinimosPorEquipe && restantes.Count > 0)
                {
                    var posicoesDoTime = equipe.Select(alunoId => posicaoDe[alunoId]).ToHashSet();
                    var escolhido = restantes.FirstOrDefault(alunoId => !posicoesDoTime.Contains(posicaoDe[alunoId]));
                    if (escolhido == Guid.Empty) escolhido = restantes[0];
                    equipe.Add(escolhido);
                    restantes.Remove(escolhido);
                }
            }

            for (var indice = 0; restantes.Count > 0; indice++)
            {
                equipes[indice % quantidade].Add(restantes[0]);
                restantes.RemoveAt(0);
            }

            return new ResultadoDoSorteio(equipes, new(), maximoDeEquipes, null);
        }

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

            // Quem não coube em um time completo entra como reserva em algum time, ninguém fica de fora.
            var reservas = Embaralhar(embaralhados.Values.SelectMany(lista => lista.Skip(quantidade)).ToList(), aleatorio);
            for (var indice = 0; indice < reservas.Count; indice++)
            {
                equipes[indice % quantidade].Add(reservas[indice]);
            }

            return new ResultadoDoSorteio(equipes, new(), maximoDeEquipes, null);
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
