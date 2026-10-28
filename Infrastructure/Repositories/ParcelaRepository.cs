using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal sealed class LinhaDeParcela
    {
        public Parcela Parcela { get; set; } = null!;
        public Contrato Contrato { get; set; } = null!;
        public Aluno Aluno { get; set; } = null!;
    }

    public class ParcelaRepository : IParcelaRepository
    {
        private readonly AppDbContext _context;

        public ParcelaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarVariasAsync(IEnumerable<Parcela> parcelas)
        {
            await _context.Parcelas.AddRangeAsync(parcelas);
            await _context.SaveChangesAsync();
        }

        public async Task<Parcela?> ObterPorIdAsync(Guid id)
        {
            return await _context.Parcelas
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Parcela>> ObterTodasAsync()
        {
            return await _context.Parcelas
                .ToListAsync();
        }

        public async Task AtualizarAsync(Parcela parcela)
        {
            _context.Parcelas.Update(parcela);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Parcela>> ObterComFiltrosAsync(Guid? alunoId, string? status, int? mes)
        {
            var query = _context.Parcelas.AsQueryable();

            if (mes.HasValue)
            {
                query = query.Where(p => p.DataVencimento.Month == mes.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<StatusParcela>(status, true, out var statusEnum))
                {
                    query = query.Where(p => p.Status == statusEnum);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ParcelaDetalhada>> ObterDetalhadasAsync(Guid? alunoId, string? status, int? mes, DateOnly hoje)
        {
            var consulta = Detalhadas()
                .Where(x => x.Contrato.Status == StatusContrato.Ativo);

            if (alunoId.HasValue)
                consulta = consulta.Where(x => x.Aluno.Id == alunoId.Value);

            if (mes.HasValue)
                consulta = consulta.Where(x => x.Parcela.DataVencimento.Month == mes.Value
                                            && x.Parcela.DataVencimento.Year == hoje.Year);

            switch (status?.Trim().ToLowerInvariant())
            {
                case "pago":
                    consulta = consulta.Where(x => x.Parcela.Status == StatusParcela.Pago);
                    break;
                case "atrasado":
                    consulta = consulta.Where(x => x.Parcela.Status != StatusParcela.Pago && x.Parcela.DataVencimento < hoje);
                    break;
                case "pendente":
                    consulta = consulta.Where(x => x.Parcela.Status != StatusParcela.Pago && x.Parcela.DataVencimento >= hoje);
                    break;
            }

            return await consulta
                .OrderBy(x => x.Parcela.DataVencimento)
                .ThenBy(x => x.Aluno.NomeCompleto)
                .Select(x => new ParcelaDetalhada(
                    x.Parcela,
                    x.Aluno.Id,
                    x.Aluno.NomeCompleto,
                    x.Aluno.ResponsavelNome,
                    x.Aluno.Telefone,
                    x.Contrato.Status))
                .ToListAsync();
        }

        public async Task<ParcelaDetalhada?> ObterDetalhadaPorIdAsync(Guid id)
        {
            return await Detalhadas()
                .Where(x => x.Parcela.Id == id)
                .Select(x => new ParcelaDetalhada(
                    x.Parcela,
                    x.Aluno.Id,
                    x.Aluno.NomeCompleto,
                    x.Aluno.ResponsavelNome,
                    x.Aluno.Telefone,
                    x.Contrato.Status))
                .FirstOrDefaultAsync();
        }

        private IQueryable<LinhaDeParcela> Detalhadas()
        {
            return from p in _context.Parcelas.AsNoTracking()
                   join c in _context.Contratos.AsNoTracking() on p.ContratoId equals c.Id
                   join a in _context.Alunos.AsNoTracking() on c.AlunoId equals a.Id
                   select new LinhaDeParcela { Parcela = p, Contrato = c, Aluno = a };
        }
    }
}