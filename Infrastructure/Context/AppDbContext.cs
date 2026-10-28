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

        public DbSet<Professor> Professores => Set<Professor>();
        public DbSet<Turma> Turmas => Set<Turma>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Aluno> Alunos => Set<Aluno>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Coordenador> Coordenadores => Set<Coordenador>();
        public DbSet<Campeonato> Campeonatos => Set<Campeonato>();
        public DbSet<ParticipacaoCampeonato> ParticipacoesCampeonato => Set<ParticipacaoCampeonato>();
        public DbSet<Contrato> Contratos => Set<Contrato>();
        public DbSet<Parcela> Parcelas => Set<Parcela>();
        public DbSet<Reposicao> Reposicoes => Set<Reposicao>();
        public DbSet<JogoCampeonato> JogosCampeonato => Set<JogoCampeonato>();

        public DbSet<EstatisticaJogo> EstatisticasJogo => Set<EstatisticaJogo>();

        public DbSet<EquipeCampeonato> EquipesCampeonato => Set<EquipeCampeonato>();

        public DbSet<CodigoRecuperacaoSenha> CodigosRecuperacaoSenha => Set<CodigoRecuperacaoSenha>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Professor>().ToTable("Professor");
            modelBuilder.Entity<Turma>().ToTable("Turma");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);


        }
    }
}