namespace Domain.Entities
{
    public class Coordenador
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public Guid UsuarioId { get; private set; }

       
        public Usuario Usuario { get; private set; } = null!;

        
        protected Coordenador() { }

        public Coordenador(string nome, Guid usuarioId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do coordenador é obrigatório.", nameof(nome));

            Id = Guid.NewGuid();
            Nome = nome;
            UsuarioId = usuarioId;
        }

        public void Atualizar(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do coordenador é obrigatório.", nameof(nome));

            Nome = nome;
        }
    }
}