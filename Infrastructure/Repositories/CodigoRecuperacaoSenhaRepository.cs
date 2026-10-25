using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CodigoRecuperacaoSenhaRepository : ICodigoRecuperacaoSenhaRepository
    {
        private readonly AppDbContext _context;

        public CodigoRecuperacaoSenhaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(CodigoRecuperacaoSenha codigo)
        {
            _context.CodigosRecuperacaoSenha.Add(codigo);
            await _context.SaveChangesAsync();
        }

        public async Task<CodigoRecuperacaoSenha?> ObterMaisRecenteDoUsuarioAsync(Guid usuarioId)
        {
            return await _context.CodigosRecuperacaoSenha
                .Where(codigo => codigo.UsuarioId == usuarioId)
                .OrderByDescending(codigo => codigo.CriadoEm)
                .FirstOrDefaultAsync();
        }

        public async Task<CodigoRecuperacaoSenha?> ObterPorTokenHashAsync(string tokenHash)
        {
            return await _context.CodigosRecuperacaoSenha
                .FirstOrDefaultAsync(codigo => codigo.TokenHash == tokenHash);
        }

        public async Task InvalidarPendentesDoUsuarioAsync(Guid usuarioId)
        {
            var pendentes = await _context.CodigosRecuperacaoSenha
                .Where(codigo => codigo.UsuarioId == usuarioId && !codigo.Usado)
                .ToListAsync();

            foreach (var pendente in pendentes)
            {
                pendente.MarcarComoUsado();
            }

            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(CodigoRecuperacaoSenha codigo)
        {
            _context.CodigosRecuperacaoSenha.Update(codigo);
            await _context.SaveChangesAsync();
        }
    }
}