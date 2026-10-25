using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarReposicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reposicoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlunoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TurmaOrigemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TurmaDestinoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataOriginal = table.Column<DateOnly>(type: "date", nullable: false),
                    DataReposicao = table.Column<DateOnly>(type: "date", nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reposicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reposicoes_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reposicoes_Turma_TurmaDestinoId",
                        column: x => x.TurmaDestinoId,
                        principalTable: "Turma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reposicoes_Turma_TurmaOrigemId",
                        column: x => x.TurmaOrigemId,
                        principalTable: "Turma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reposicoes_AlunoId",
                table: "Reposicoes",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reposicoes_TurmaDestinoId",
                table: "Reposicoes",
                column: "TurmaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reposicoes_TurmaOrigemId",
                table: "Reposicoes",
                column: "TurmaOrigemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reposicoes");
        }
    }
}
