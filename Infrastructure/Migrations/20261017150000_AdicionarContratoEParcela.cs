using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarContratoEParcela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlunoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "numeric", nullable: false),
                    NumParcelas = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PdfPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parcelas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    DataVencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataPagamento = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcelas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parcelas_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_AlunoId",
                table: "Contratos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcelas_ContratoId",
                table: "Parcelas",
                column: "ContratoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parcelas");

            migrationBuilder.DropTable(
                name: "Contratos");
        }
    }
}
