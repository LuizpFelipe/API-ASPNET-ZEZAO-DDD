using System;

namespace Domain.Entities
{
	public class Categoria
	{
		public Guid Id { get; private set; }
		public string Nome { get; private set; }
		public int IdadeMin { get; private set; }
		public int IdadeMax { get; private set; }

		protected Categoria() { }

		public Categoria(string nome, int idadeMin, int idadeMax)
		{
			Id = Guid.NewGuid();
			Atualizar(nome, idadeMin, idadeMax);
		}

		public void Atualizar(string nome, int idadeMin, int idadeMax)
		{
			if (string.IsNullOrWhiteSpace(nome))
				throw new ArgumentException("O nome da categoria é obrigatório.");

			if (idadeMin < 0 || idadeMax < 0 || idadeMin > idadeMax)
				throw new ArgumentException("Faixa de idade inválida.");

			Nome = nome;
			IdadeMin = idadeMin;
			IdadeMax = idadeMax;
		}
	}
}