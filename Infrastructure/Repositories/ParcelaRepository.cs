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

        // --- Adicionado para atender aos novos requisitos sem alterar o código existente ---

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
    }
}