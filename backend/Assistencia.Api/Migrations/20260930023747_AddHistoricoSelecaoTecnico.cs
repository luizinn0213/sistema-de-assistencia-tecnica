using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assistencia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricoSelecaoTecnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoricosSelecoesTecnicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SelecaoTecnicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosSelecoesTecnicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosSelecoesTecnicos_SelecoesTecnicos_SelecaoTecnicoId",
                        column: x => x.SelecaoTecnicoId,
                        principalTable: "SelecoesTecnicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosSelecoesTecnicos_SelecaoTecnicoId_DataRegistro",
                table: "HistoricosSelecoesTecnicos",
                columns: new[] { "SelecaoTecnicoId", "DataRegistro" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricosSelecoesTecnicos");
        }
    }
}
