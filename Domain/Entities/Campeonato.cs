using System;

namespace Domain.Entities
{
    public class Campeonato
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public DateOnly Data { get; private set; }
        public Guid CategoriaId { get; private set; }

        protected Campeonato() { }

        public Campeonato(string nome, DateOnly data, Guid categoriaId)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Data = data;
            CategoriaId = categoriaId;
        }

        public void AtualizarDetalhes(string nome, DateOnly data, Guid categoriaId)
        {
            Nome = nome;
            Data = data;
            CategoriaId = categoriaId;
        }
    }
}
