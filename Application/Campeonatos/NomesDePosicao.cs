using Domain.Enums;

namespace Application.Campeonatos
{
    public static class NomesDePosicao
    {
        public static readonly PosicaoJogador[] TodasAsPosicoes = Enum.GetValues<PosicaoJogador>();

        public static string Obter(PosicaoJogador posicao) => posicao switch
        {
            PosicaoJogador.Goleiro => "Goleiro",
            PosicaoJogador.Fixo => "Fixo",
            PosicaoJogador.AlaDireito => "Ala direito",
            PosicaoJogador.AlaEsquerdo => "Ala esquerdo",
            PosicaoJogador.Pivo => "Pivô",
            _ => posicao.ToString()
        };

        public static string? Obter(PosicaoJogador? posicao) => posicao.HasValue ? Obter(posicao.Value) : null;
    }
}
