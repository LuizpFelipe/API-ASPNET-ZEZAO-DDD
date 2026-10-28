using System;

namespace Domain.Entities
{
    public class ParticipacaoCampeonato
    {
        public Guid Id { get; private set; }
        public Guid CampeonatoId { get; private set; }
        public Guid AlunoId { get; private set; }
        public string? Posicao { get; private set; }
        public int JogosDisputados { get; private set; }
        public decimal Aproveitamento { get; private set; }
        public Guid? EquipeId { get; private set; }

        protected ParticipacaoCampeonato() { }

        public ParticipacaoCampeonato(Guid campeonatoId, Guid alunoId, string? posicao)
        {
            Id = Guid.NewGuid();
            CampeonatoId = campeonatoId;
            AlunoId = alunoId;
            Posicao = posicao;
        }

        public void DefinirEquipe(Guid? equipeId)
        {
            EquipeId = equipeId;
        }

        public void Atualizar(string? posicao, int jogosDisputados, decimal aproveitamento)
        {
            Posicao = posicao;
            JogosDisputados = jogosDisputados;
            Aproveitamento = aproveitamento;
        }
    }
}
