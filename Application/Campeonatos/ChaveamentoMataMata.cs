using Domain.Entities;

namespace Application.Campeonatos
{
    public record JogoPlanejado(int Fase, int Ordem, Guid? EquipeAId, Guid? EquipeBId);

    public static class ChaveamentoMataMata
    {
        public static int NumeroDeFases(int quantidadeDeEquipes)
        {
            var fases = 1;
            var tamanho = 2;
            while (tamanho < quantidadeDeEquipes)
            {
                tamanho *= 2;
                fases++;
            }
            return fases;
        }

        public static string NomeDaFase(int fase, int totalDeFases)
        {
            return (totalDeFases - fase) switch
            {
                0 => "Final",
                1 => "Semifinal",
                2 => "Quartas de final",
                3 => "Oitavas de final",
                _ => $"Fase {fase}"
            };
        }

        public static List<JogoPlanejado> Gerar(IReadOnlyList<Guid> equipes, Random aleatorio)
        {
            if (equipes.Count < 2)
            {
                throw new ArgumentException("São necessárias ao menos 2 equipes para gerar os jogos.", nameof(equipes));
            }

            var totalDeFases = NumeroDeFases(equipes.Count);
            var tamanhoDaChave = 1 << totalDeFases;

            var embaralhadas = equipes.ToList();
            for (var indice = embaralhadas.Count - 1; indice > 0; indice--)
            {
                var sorteado = aleatorio.Next(indice + 1);
                (embaralhadas[indice], embaralhadas[sorteado]) = (embaralhadas[sorteado], embaralhadas[indice]);
            }

            var vagas = new Guid?[tamanhoDaChave];
            for (var indice = 0; indice < embaralhadas.Count; indice++)
            {
                vagas[indice] = embaralhadas[indice];
            }

            var jogos = new List<JogoPlanejado>();
            for (var ordem = 0; ordem < tamanhoDaChave / 2; ordem++)
            {
                jogos.Add(new JogoPlanejado(1, ordem, vagas[ordem], vagas[tamanhoDaChave - 1 - ordem]));
            }

            for (var fase = 2; fase <= totalDeFases; fase++)
            {
                var jogosDaFase = tamanhoDaChave >> fase;
                for (var ordem = 0; ordem < jogosDaFase; ordem++)
                {
                    jogos.Add(new JogoPlanejado(fase, ordem, null, null));
                }
            }

            return jogos;
        }

        public static bool Avancar(IEnumerable<JogoCampeonato> jogos, JogoCampeonato jogoFinalizado)
        {
            if (jogoFinalizado.EquipeVencedoraId == null)
            {
                return false;
            }

            var proximoJogo = jogos.FirstOrDefault(jogo =>
                jogo.Fase == jogoFinalizado.Fase + 1 && jogo.Ordem == jogoFinalizado.Ordem / 2);

            if (proximoJogo == null)
            {
                return false;
            }

            proximoJogo.DefinirEquipe(jogoFinalizado.Ordem % 2 == 0, jogoFinalizado.EquipeVencedoraId.Value);
            return true;
        }
    }
}
