using System;
using System.Linq;
using Domain.Entities;
// using Infrastructure.Context;

namespace Infrastructure.Persistence
{
    public static class TurmaSeeder
    {
        // Exemplo estrutural para aplicar no Startup/Program.cs
        public static void Seed(/* ZezaoContext context, Guid categoriaPadraoId, Guid professorPadraoId */)
        {
            // if (!context.Turmas.Any())
            // {
            //     var turmas = new[]
            //     {
            //         new Turma("Turma A", "Segunda e Quarta", "18:00", categoriaPadraoId, professorPadraoId),
            //         new Turma("Turma B", "Segunda e Quarta", "19:00", categoriaPadraoId, professorPadraoId),
            //         new Turma("Turma C", "Terça e Quinta", "18:00", categoriaPadraoId, professorPadraoId),
            //         new Turma("Turma D", "Terça e Quinta", "19:00", categoriaPadraoId, professorPadraoId)
            //     };
            //     
            //     context.Turmas.AddRange(turmas);
            //     context.SaveChanges();
            // }
        }
    }
}