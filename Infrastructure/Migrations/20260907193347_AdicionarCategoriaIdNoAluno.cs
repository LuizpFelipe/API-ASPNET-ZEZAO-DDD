using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCategoriaIdNoAluno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Alunos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_CategoriaId",
                table: "Alunos",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alunos_Categorias_CategoriaId",
                table: "Alunos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alunos_Categorias_CategoriaId",
                table: "Alunos");

            migrationBuilder.DropIndex(
                name: "IX_Alunos_CategoriaId",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Alunos");
        }
    }
}
