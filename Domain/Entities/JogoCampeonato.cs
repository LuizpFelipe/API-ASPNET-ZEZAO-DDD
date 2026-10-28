using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class JogoCampeonato
    {
        public Guid Id { get; private set; }
        public Guid CampeonatoId { get; private set; }
        public int Fase { get; private set; }
        public int Ordem { get; private set; }
        public Guid? EquipeAId { get; private set; }
        public Guid? EquipeBId { get; private set; }
        public int GolsA { get; private set; }
        public int GolsB { get; private set; }
        public StatusJogo Status { get; private set; }
        public Guid? EquipeVencedoraId { get; private set; }
        public bool Bye { get; private set; }

        protected JogoCampeonato() { }

        public JogoCampeonato(Guid campeonatoId, int fase, int ordem)
        {
            Id = Guid.NewGuid();
            CampeonatoId = campeonatoId;
            Fase = fase;
            Ordem = ordem;
            Status = StatusJogo.Aguardando;
        }

        public void DefinirEquipe(bool ehLadoA, Guid equipeId)
        {
            if (ehLadoA) EquipeAId = equipeId;
            else EquipeBId = equipeId;

            if (EquipeAId.HasValue && EquipeBId.HasValue && Status == StatusJogo.Aguardando)
                Status = StatusJogo.Pendente;
        }

        public void FinalizarPorBye(Guid equipeVencedoraId)
        {
            Bye = true;
            EquipeVencedoraId = equipeVencedoraId;
            Status = StatusJogo.Finalizado;
        }

        public void AtualizarPlacar(int golsA, int golsB)
        {
            GolsA = golsA;
            GolsB = golsB;
        }

        public void Finalizar(Guid equipeVencedoraId)
        {
            EquipeVencedoraId = equipeVencedoraId;
            Status = StatusJogo.Finalizado;
        }
    }
}
