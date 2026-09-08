using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;

namespace Infrastructure.Persistence.Seeders
{
	public static class CategoriaSeeder
	{
		public static async Task SeedAsync(AppDbContext context)
		{
			if (!await context.Set<Categoria>().AnyAsync())
			{
				var categorias = new[]
				{
					new Categoria("Sub-11", 9, 10),
					new Categoria("Sub-12", 11, 11),
					new Categoria("Sub-13", 12, 12),
					new Categoria("Sub-14", 13, 14)
				};

				await context.Set<Categoria>().AddRangeAsync(categorias);
				await context.SaveChangesAsync();
			}
		}
	}
}