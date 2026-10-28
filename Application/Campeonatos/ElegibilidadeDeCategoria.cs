using Domain.Entities;

namespace Application.Campeonatos
{
    public static class ElegibilidadeDeCategoria
    {
        public static Categoria? ObterCategoriaImediatamenteAbaixo(Categoria categoriaDoCampeonato, IEnumerable<Categoria> todasAsCategorias)
        {
            return todasAsCategorias
                .Where(categoria => categoria.IdadeMax < categoriaDoCampeonato.IdadeMin)
                .OrderByDescending(categoria => categoria.IdadeMax)
                .FirstOrDefault();
        }

        public static HashSet<Guid> ObterCategoriasElegiveis(Categoria categoriaDoCampeonato, IEnumerable<Categoria> todasAsCategorias)
        {
            var elegiveis = new HashSet<Guid> { categoriaDoCampeonato.Id };

            var categoriaAbaixo = ObterCategoriaImediatamenteAbaixo(categoriaDoCampeonato, todasAsCategorias);
            if (categoriaAbaixo != null)
            {
                elegiveis.Add(categoriaAbaixo.Id);
            }

            return elegiveis;
        }
    }
}
