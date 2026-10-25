using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Persistence
{
    public static class UsuarioSeeder
    {
        public static void Seed(AppDbContext context, ISecurityService securityService,
            string emailDoCoordenador, string? emailDoProfessorDeExemplo, string senhaInicial)
        {
            if (context.Usuarios.Any())
            {
                return;
            }

            var coordenador = new Usuario(
                "coordenacao",
                emailDoCoordenador,
                securityService.HashPassword(senhaInicial),
                PerfilUsuario.Coordenador
            );
            context.Usuarios.Add(coordenador);

            if (!string.IsNullOrWhiteSpace(emailDoProfessorDeExemplo))
            {
                var professorDeExemplo = new Usuario(
                    "professor",
                    emailDoProfessorDeExemplo,
                    securityService.HashPassword(senhaInicial),
                    PerfilUsuario.Professor
                );
                context.Usuarios.Add(professorDeExemplo);
            }

            context.SaveChanges();
        }
    }
}