using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CampeonatoComTimesEJogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EquipeId",
                table: "ParticipacoesCampeonato",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Posicao",
                table: "Alunos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EquipesCampeonato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampeonatoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipesCampeonato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipesCampeonato_Campeonatos_CampeonatoId",
                        column: x => x.CampeonatoId,
                        principalTable: "Campeonatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JogosCampeonato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampeonatoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fase = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    EquipeAId = table.Column<Guid>(type: "uuid", nullable: true),
                    EquipeBId = table.Column<Guid>(type: "uuid", nullable: true),
                    GolsA = table.Column<int>(type: "integer", nullable: false),
                    GolsB = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EquipeVencedoraId = table.Column<Guid>(type: "uuid", nullable: true),
                    Bye = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JogosCampeonato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JogosCampeonato_Campeonatos_CampeonatoId",
                        column: x => x.CampeonatoId,
                        principalTable: "Campeonatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JogosCampeonato_EquipesCampeonato_EquipeAId",
                        column: x => x.EquipeAId,
                        principalTable: "EquipesCampeonato",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JogosCampeonato_EquipesCampeonato_EquipeBId",
                        column: x => x.EquipeBId,
                        principalTable: "EquipesCampeonato",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JogosCampeonato_EquipesCampeonato_EquipeVencedoraId",
                        column: x => x.EquipeVencedoraId,
                        principalTable: "EquipesCampeonato",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EstatisticasJogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlunoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gols = table.Column<int>(type: "integer", nullable: false),
                    Assistencias = table.Column<int>(type: "integer", nullable: false),
                    PassesCertos = table.Column<int>(type: "integer", nullable: false),
                    Desarmes = table.Column<int>(type: "integer", nullable: false),
                    Defesas = table.Column<int>(type: "integer", nullable: false),
                    Faltas = table.Column<int>(type: "integer", nullable: false),
                    CartoesAmarelos = table.Column<int>(type: "integer", nullable: false),
                    CartoesVermelhos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstatisticasJogo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstatisticasJogo_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstatisticasJogo_JogosCampeonato_JogoId",
                        column: x => x.JogoId,
                        principalTable: "JogosCampeonato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesCampeonato_EquipeId",
                table: "ParticipacoesCampeonato",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipesCampeonato_CampeonatoId_Nome",
                table: "EquipesCampeonato",
                columns: new[] { "CampeonatoId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasJogo_AlunoId",
                table: "EstatisticasJogo",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasJogo_JogoId_AlunoId",
                table: "EstatisticasJogo",
                columns: new[] { "JogoId", "AlunoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JogosCampeonato_CampeonatoId_Fase_Ordem",
                table: "JogosCampeonato",
                columns: new[] { "CampeonatoId", "Fase", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JogosCampeonato_EquipeAId",
                table: "JogosCampeonato",
                column: "EquipeAId");

            migrationBuilder.CreateIndex(
                name: "IX_JogosCampeonato_EquipeBId",
                table: "JogosCampeonato",
                column: "EquipeBId");

            migrationBuilder.CreateIndex(
                name: "IX_JogosCampeonato_EquipeVencedoraId",
                table: "JogosCampeonato",
                column: "EquipeVencedoraId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipacoesCampeonato_EquipesCampeonato_EquipeId",
                table: "ParticipacoesCampeonato",
                column: "EquipeId",
                principalTable: "EquipesCampeonato",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipacoesCampeonato_EquipesCampeonato_EquipeId",
                table: "ParticipacoesCampeonato");

            migrationBuilder.DropTable(
                name: "EstatisticasJogo");

            migrationBuilder.DropTable(
                name: "JogosCampeonato");

            migrationBuilder.DropTable(
                name: "EquipesCampeonato");

            migrationBuilder.DropIndex(
                name: "IX_ParticipacoesCampeonato_EquipeId",
                table: "ParticipacoesCampeonato");

            migrationBuilder.DropColumn(
                name: "EquipeId",
                table: "ParticipacoesCampeonato");

            migrationBuilder.DropColumn(
                name: "Posicao",
                table: "Alunos");
        }
    }
}
