using System;

namespace Domain.Entities
{
    public class Professor
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Telefone { get; private set; }
        public Guid UsuarioId { get; private set; }

        public Professor(string nome, string telefone, Guid usuarioId)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Telefone = telefone;
            UsuarioId = usuarioId;
        }

        public void AtualizarDetalhes(string nome, string telefone)
        {
            Nome = nome;
            Telefone = telefone;
        }
    }
}