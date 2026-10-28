namespace Application.Request
{
    public class CriarEquipeRequestDTO
    {
        public string? Nome { get; set; }
        public List<Guid> AlunoIds { get; set; } = new();
    }

    public class SortearEquipesRequestDTO
    {
        public int? Quantidade { get; set; }
    }

    public class EstatisticaDoJogadorRequestDTO
    {
        public Guid AlunoId { get; set; }
        public int Gols { get; set; }
        public int Assistencias { get; set; }
        public int PassesCertos { get; set; }
        public int Desarmes { get; set; }
        public int Defesas { get; set; }
        public int Faltas { get; set; }
        public int CartoesAmarelos { get; set; }
        public int CartoesVermelhos { get; set; }
    }

    public class SalvarEstatisticasRequestDTO
    {
        public List<EstatisticaDoJogadorRequestDTO> Jogadores { get; set; } = new();
    }
}
