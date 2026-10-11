using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CampeonatoRepository : ICampeonatoRepository
    {
        private readonly AppDbContext _context;

        public CampeonatoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Campeonato>> ObterTodosAsync()
        {
            return await _context.Campeonatos.AsNoTracking().ToListAsync();
        }

        public async Task<Campeonato?> ObterPorIdAsync(Guid id)
        {
            return await _context.Campeonatos.FindAsync(id);
        }

        public async Task AdicionarAsync(Campeonato campeonato)
        {
            await _context.Campeonatos.AddAsync(campeonato);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Campeonato campeonato)
        {
            _context.Campeonatos.Update(campeonato);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Campeonato campeonato)
        {
            _context.Campeonatos.Remove(campeonato);
            await _context.SaveChangesAsync();
        }

        public async Task AdicionarParticipacaoAsync(ParticipacaoCampeonato participacao)
        {
            await _context.ParticipacoesCampeonato.AddAsync(participacao);
            await _context.SaveChangesAsync();
        }

        public async Task<ParticipacaoCampeonato?> ObterParticipacaoPorIdAsync(Guid id)
        {
            return await _context.ParticipacoesCampeonato.FindAsync(id);
        }

        public async Task AtualizarParticipacaoAsync(ParticipacaoCampeonato participacao)
        {
            _context.ParticipacoesCampeonato.Update(participacao);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ParticipacaoCampeonato>> ObterParticipacoesPorCampeonatoAsync(Guid campeonatoId)
        {
            return await _context.ParticipacoesCampeonato
                .AsNoTracking()
                .Where(p => p.CampeonatoId == campeonatoId)
                .ToListAsync();
        }
    }
}
