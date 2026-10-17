namespace Domain.Entities
{
    public class Presenca
    {
        public Guid Id { get; private set; }
        public Guid AlunoId { get; private set; }
        public Guid TurmaId { get; private set; }
        public DateOnly Data { get; private set; }
        public bool Presente { get; private set; }

       
        protected Presenca() { }

        public Presenca(Guid alunoId, Guid turmaId, DateOnly data, bool presente)
        {
            Id = Guid.NewGuid();
            AlunoId = alunoId;
            TurmaId = turmaId;
            Data = data;
            Presente = presente;
        }

        public void Marcar(bool presente)
        {
            Presente = presente;
        }
    }
}