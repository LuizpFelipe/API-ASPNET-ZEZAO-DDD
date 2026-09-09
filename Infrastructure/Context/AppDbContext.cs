using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Professor> Professores => Set<Professor>();
        public DbSet<Turma> Turmas => Set<Turma>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Aluno> Alunos => Set<Aluno>();
        public DbSet<Categoria> Categorias => Set<Categoria>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Professor>().ToTable("Professor");
            modelBuilder.Entity<Turma>().ToTable("Turma");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

           
        }
    }
}