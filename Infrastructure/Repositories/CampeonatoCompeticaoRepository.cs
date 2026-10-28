using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CampeonatoCompeticaoRepository : ICampeonatoCompeticaoRepository
    {
        private readonly AppDbContext _context;

        public CampeonatoCompeticaoRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<EquipeCampeonato>> ObterEquipesAsync(Guid campeonatoId) =>
            _context.EquipesCampeonato.Where(e => e.CampeonatoId == campeonatoId).ToListAsync();

        public Task<EquipeCampeonato?> ObterEquipePorIdAsync(Guid equipeId) =>
            _context.EquipesCampeonato.FirstOrDefaultAsync(e => e.Id == equipeId);

        public async Task AdicionarEquipesAsync(IEnumerable<EquipeCampeonato> equipes, IEnumerable<ParticipacaoCampeonato> participacoesAtualizadas)
        {
            await _context.EquipesCampeonato.AddRangeAsync(equipes);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverEquipeAsync(EquipeCampeonato equipe, IEnumerable<ParticipacaoCampeonato> participacoesLiberadas)
        {
            _context.EquipesCampeonato.Remove(equipe);
            await _context.SaveChangesAsync();
        }

        public Task<List<ParticipacaoCampeonato>> ObterParticipacoesAsync(Guid campeonatoId) =>
            _context.ParticipacoesCampeonato.Where(p => p.CampeonatoId == campeonatoId).ToListAsync();

        public Task<ParticipacaoCampeonato?> ObterParticipacaoPorIdAsync(Guid participacaoId) =>
            _context.ParticipacoesCampeonato.FirstOrDefaultAsync(p => p.Id == participacaoId);

        public async Task RemoverParticipacaoAsync(ParticipacaoCampeonato participacao)
        {
            _context.ParticipacoesCampeonato.Remove(participacao);
            await _context.SaveChangesAsync();
        }

        public Task<List<JogoCampeonato>> ObterJogosAsync(Guid campeonatoId) =>
            _context.JogosCampeonato.Where(j => j.CampeonatoId == campeonatoId).ToListAsync();

        public Task<JogoCampeonato?> ObterJogoPorIdAsync(Guid jogoId) =>
            _context.JogosCampeonato.FirstOrDefaultAsync(j => j.Id == jogoId);

        public async Task AdicionarJogosAsync(IEnumerable<JogoCampeonato> jogos)
        {
            await _context.JogosCampeonato.AddRangeAsync(jogos);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarJogosAsync(IEnumerable<JogoCampeonato> jogos)
        {
            foreach (var jogo in jogos)
            {
                if (_context.Entry(jogo).State == EntityState.Detached)
                    _context.JogosCampeonato.Update(jogo);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoverJogosDoCampeonatoAsync(Guid campeonatoId)
        {
            var jogos = await _context.JogosCampeonato.Where(j => j.CampeonatoId == campeonatoId).ToListAsync();
            _context.JogosCampeonato.RemoveRange(jogos);
            await _context.SaveChangesAsync();
        }

        public Task<List<EstatisticaJogo>> ObterEstatisticasDoJogoAsync(Guid jogoId) =>
            _context.EstatisticasJogo.AsNoTracking().Where(e => e.JogoId == jogoId).ToListAsync();

        public Task<List<EstatisticaJogo>> ObterEstatisticasDoCampeonatoAsync(Guid campeonatoId) =>
            _context.EstatisticasJogo.AsNoTracking()
                .Where(e => _context.JogosCampeonato.Any(j => j.Id == e.JogoId && j.CampeonatoId == campeonatoId))
                .ToListAsync();

        public async Task SubstituirEstatisticasDoJogoAsync(Guid jogoId, IEnumerable<EstatisticaJogo> estatisticas)
        {
            var antigas = await _context.EstatisticasJogo.Where(e => e.JogoId == jogoId).ToListAsync();
            _context.EstatisticasJogo.RemoveRange(antigas);
            await _context.EstatisticasJogo.AddRangeAsync(estatisticas);
            await _context.SaveChangesAsync();
        }
    }
}
