using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Persistence
{
    public static class UsuarioSeeder
    {
        public static void Seed(AppDbContext context, ISecurityService securityService)
        {
            if (context.Usuarios.Any())
            {
                return;
            }

            var coordenador = new Usuario(
                "coordenacao",
                securityService.HashPassword("user123"),
                PerfilUsuario.Coordenador
            );

            var professor = new Usuario(
                "professor",
                securityService.HashPassword("user123"),
                PerfilUsuario.Professor
            );

            context.Usuarios.AddRange(coordenador, professor);
            context.SaveChanges();
        }
    }
}