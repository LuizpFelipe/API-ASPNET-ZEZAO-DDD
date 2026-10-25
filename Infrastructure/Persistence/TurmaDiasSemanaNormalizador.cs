using Domain.Entities;
using Domain.Utilitarios;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class TurmaDiasSemanaNormalizador
{
    public static async Task<IReadOnlyList<string>> NormalizarAsync(AppDbContext contexto)
    {
        var turmasSemConversao = new List<string>();
        var turmas = await contexto.Set<Turma>().ToListAsync();
        var houveAlteracao = false;

        foreach (var turma in turmas)
        {
            var diasPadronizados = DiasDaSemana.Padronizar(turma.DiasSemana);

            if (diasPadronizados.Length == 0)
            {
                turmasSemConversao.Add($"{turma.Nome}: \"{turma.DiasSemana}\"");
                continue;
            }

            if (diasPadronizados == turma.DiasSemana) continue;

            turma.AtualizarDetalhes(turma.Nome, diasPadronizados, turma.Horario, turma.CategoriaId, turma.ProfessorId);
            houveAlteracao = true;
        }

        if (houveAlteracao) await contexto.SaveChangesAsync();

        return turmasSemConversao;
    }
}