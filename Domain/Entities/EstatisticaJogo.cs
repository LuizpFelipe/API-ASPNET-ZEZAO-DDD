using System;

namespace Domain.Entities
{
    public class EstatisticaJogo
    {
        public Guid Id { get; private set; }
        public Guid JogoId { get; private set; }
        public Guid AlunoId { get; private set; }
        public int Gols { get; private set; }
        public int Assistencias { get; private set; }
        public int PassesCertos { get; private set; }
        public int Desarmes { get; private set; }
        public int Defesas { get; private set; }
        public int Faltas { get; private set; }
        public int CartoesAmarelos { get; private set; }
        public int CartoesVermelhos { get; private set; }

        protected EstatisticaJogo() { }

        public EstatisticaJogo(Guid jogoId, Guid alunoId, int gols, int assistencias, int passesCertos,
                               int desarmes, int defesas, int faltas, int cartoesAmarelos, int cartoesVermelhos)
        {
            Id = Guid.NewGuid();
            JogoId = jogoId;
            AlunoId = alunoId;
            Gols = gols;
            Assistencias = assistencias;
            PassesCertos = passesCertos;
            Desarmes = desarmes;
            Defesas = defesas;
            Faltas = faltas;
            CartoesAmarelos = cartoesAmarelos;
            CartoesVermelhos = cartoesVermelhos;
        }
    }
}
