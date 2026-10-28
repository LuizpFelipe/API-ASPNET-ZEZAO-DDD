using System;

namespace Application.Response
{
    public class ParticipacaoCampeonatoResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
        public object? Data { get; set; }
    }

    public class ParticipacaoCampeonatoDataDTO
    {
        public Guid Id { get; set; }
        public Guid CampeonatoId { get; set; }
        public Guid AlunoId { get; set; }
        public string? Posicao { get; set; }
        public int JogosDisputados { get; set; }
        public decimal Aproveitamento { get; set; }
        public string? AlunoNome { get; set; }
        public string? PosicaoDeJogo { get; set; }
        public Guid? EquipeId { get; set; }
    }
}
