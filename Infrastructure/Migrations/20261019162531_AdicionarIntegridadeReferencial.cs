using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIntegridadeReferencial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alunos_Categorias_CategoriaId",
                table: "Alunos");

            migrationBuilder.DropForeignKey(
                name: "FK_Campeonatos_Categorias_CategoriaId",
                table: "Campeonatos");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipacoesCampeonato_Alunos_AlunoId",
                table: "ParticipacoesCampeonato");

            migrationBuilder.DropForeignKey(
                name: "FK_Presencas_Alunos_AlunoId",
                table: "Presencas");

            migrationBuilder.DropForeignKey(
                name: "FK_Presencas_Turma_TurmaId",
                table: "Presencas");

            migrationBuilder.CreateIndex(
                name: "IX_Turma_CategoriaId",
                table: "Turma",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Turma_ProfessorId",
                table: "Turma",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Professor_UsuarioId",
                table: "Professor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_TurmaId",
                table: "Alunos",
                column: "TurmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alunos_Categorias_CategoriaId",
                table: "Alunos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alunos_Turma_TurmaId",
                table: "Alunos",
                column: "TurmaId",
                principalTable: "Turma",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campeonatos_Categorias_CategoriaId",
                table: "Campeonatos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipacoesCampeonato_Alunos_AlunoId",
                table: "ParticipacoesCampeonato",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presencas_Alunos_AlunoId",
                table: "Presencas",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presencas_Turma_TurmaId",
                table: "Presencas",
                column: "TurmaId",
                principalTable: "Turma",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Professor_Usuarios_UsuarioId",
                table: "Professor",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turma_Categorias_CategoriaId",
                table: "Turma",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turma_Professor_ProfessorId",
                table: "Turma",
                column: "ProfessorId",
                principalTable: "Professor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alunos_Categorias_CategoriaId",
                table: "Alunos");

            migrationBuilder.DropForeignKey(
                name: "FK_Alunos_Turma_TurmaId",
                table: "Alunos");

            migrationBuilder.DropForeignKey(
                name: "FK_Campeonatos_Categorias_CategoriaId",
                table: "Campeonatos");

            migrationBuilder.DropForeignKey(
                name: "FK_ParticipacoesCampeonato_Alunos_AlunoId",
                table: "ParticipacoesCampeonato");

            migrationBuilder.DropForeignKey(
                name: "FK_Presencas_Alunos_AlunoId",
                table: "Presencas");

            migrationBuilder.DropForeignKey(
                name: "FK_Presencas_Turma_TurmaId",
                table: "Presencas");

            migrationBuilder.DropForeignKey(
                name: "FK_Professor_Usuarios_UsuarioId",
                table: "Professor");

            migrationBuilder.DropForeignKey(
                name: "FK_Turma_Categorias_CategoriaId",
                table: "Turma");

            migrationBuilder.DropForeignKey(
                name: "FK_Turma_Professor_ProfessorId",
                table: "Turma");

            migrationBuilder.DropIndex(
                name: "IX_Turma_CategoriaId",
                table: "Turma");

            migrationBuilder.DropIndex(
                name: "IX_Turma_ProfessorId",
                table: "Turma");

            migrationBuilder.DropIndex(
                name: "IX_Professor_UsuarioId",
                table: "Professor");

            migrationBuilder.DropIndex(
                name: "IX_Alunos_TurmaId",
                table: "Alunos");

            migrationBuilder.AddForeignKey(
                name: "FK_Alunos_Categorias_CategoriaId",
                table: "Alunos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Campeonatos_Categorias_CategoriaId",
                table: "Campeonatos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipacoesCampeonato_Alunos_AlunoId",
                table: "ParticipacoesCampeonato",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Presencas_Alunos_AlunoId",
                table: "Presencas",
                column: "AlunoId",
                principalTable: "Alunos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Presencas_Turma_TurmaId",
                table: "Presencas",
                column: "TurmaId",
                principalTable: "Turma",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
