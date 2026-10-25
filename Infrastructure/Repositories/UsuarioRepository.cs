using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObterPorNomeUsuarioAsync(string nomeUsuario)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NomeUsuario == nomeUsuario);
        }

        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<Usuario>> ObterPorIdsAsync(IEnumerable<Guid> identificadores)
        {
            var identificadoresEmLista = identificadores.ToList();

            return await _context.Usuarios
                .Where(u => identificadoresEmLista.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AtualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }
}