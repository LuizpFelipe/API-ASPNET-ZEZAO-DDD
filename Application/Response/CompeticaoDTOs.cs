using System.Text.Json.Serialization;

namespace Application.Response
{
    public class RespostaDaCompeticao<T>
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public T? Data { get; set; }

        [JsonIgnore]
        public bool NaoEncontrado { get; set; }

        public static RespostaDaCompeticao<T> Ok(T data, string message = "") =>
            new() { Status = true, Message = message, Data = data };

        public static RespostaDaCompeticao<T> Falha(string message, bool naoEncontrado = false) =>
            new() { Status = false, Message = message, NaoEncontrado = naoEncontrado };
    }

    public class AlunoElegivelDTO
    {
        public Guid AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int? Posicao { get; set; }
        public string? PosicaoNome { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
    }

    public class JogadorDaEquipeDTO
    {
        public Guid AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int? Posicao { get; set; }
        public string? PosicaoNome { get; set; }
    }

    public class EquipeDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<JogadorDaEquipeDTO> Jogadores { get; set; } = new();
    }

    public class JogoDTO
    {
        public Guid Id { get; set; }
        public int Fase { get; set; }
        public int Ordem { get; set; }
        public string NomeDaFase { get; set; } = string.Empty;
        public Guid? EquipeAId { get; set; }
        public string? EquipeANome { get; set; }
        public Guid? EquipeBId { get; set; }
        public string? EquipeBNome { get; set; }
        public int GolsA { get; set; }
        public int GolsB { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? EquipeVencedoraId { get; set; }
        public string? EquipeVencedoraNome { get; set; }
        public bool Bye { get; set; }
    }

    public class EstatisticaDoJogadorDTO
    {
        public Guid AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int? Posicao { get; set; }
        public string? PosicaoNome { get; set; }
        public Guid EquipeId { get; set; }
        public int Gols { get; set; }
        public int Assistencias { get; set; }
        public int PassesCertos { get; set; }
        public int Desarmes { get; set; }
        public int Defesas { get; set; }
        public int Faltas { get; set; }
        public int CartoesAmarelos { get; set; }
        public int CartoesVermelhos { get; set; }
    }

    public class EstatisticasDoJogoDTO
    {
        public JogoDTO Jogo { get; set; } = new();
        public List<EstatisticaDoJogadorDTO> Jogadores { get; set; } = new();
    }

    public class LinhaDoRankingDTO
    {
        public int Colocacao { get; set; }
        public Guid AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? PosicaoNome { get; set; }
        public string? EquipeNome { get; set; }
        public int Jogos { get; set; }
        public int Gols { get; set; }
        public int Assistencias { get; set; }
        public int PassesCertos { get; set; }
        public int Desarmes { get; set; }
        public int Defesas { get; set; }
        public int Faltas { get; set; }
        public int CartoesAmarelos { get; set; }
        public int CartoesVermelhos { get; set; }
        public decimal Pontos { get; set; }
    }

    public class RankingDoCampeonatoDTO
    {
        public bool Finalizado { get; set; }
        public Guid? CampeaId { get; set; }
        public string? CampeaNome { get; set; }
        public List<LinhaDoRankingDTO> Itens { get; set; } = new();
    }
}
