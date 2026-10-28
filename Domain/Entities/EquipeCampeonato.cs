using System;

namespace Domain.Entities
{
    public class EquipeCampeonato
    {
        public Guid Id { get; private set; }
        public Guid CampeonatoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;

        protected EquipeCampeonato() { }

        public EquipeCampeonato(Guid campeonatoId, string nome)
        {
            Id = Guid.NewGuid();
            CampeonatoId = campeonatoId;
            Nome = nome;
        }
    }
}
