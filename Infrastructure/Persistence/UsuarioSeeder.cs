using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Persistence
{
    public static class UsuarioSeeder
    {
        public static void Seed(AppDbContext context, ISecurityService securityService, string emailDoCoordenador, string emailDoProfessor)
        {
            if (context.Usuarios.Any())
            {
                return;
            }

            var coordenador = new Usuario(
                "coordenacao",
                emailDoCoordenador,
                securityService.HashPassword("user123"),
                PerfilUsuario.Coordenador
            );

            var professor = new Usuario(
                "professor",
                emailDoProfessor,
                securityService.HashPassword("user123"),
                PerfilUsuario.Professor
            );

            context.Usuarios.AddRange(coordenador, professor);
            context.SaveChanges();
        }
    }
}