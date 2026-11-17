using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assistencia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diagnosticos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SolicitacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TecnicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DescricaoDefeito = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SolucaoRecomendada = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnosticos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diagnosticos_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Diagnosticos_Tecnicos_TecnicoId",
                        column: x => x.TecnicoId,
                        principalTable: "Tecnicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosticos_SolicitacaoId",
                table: "Diagnosticos",
                column: "SolicitacaoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosticos_TecnicoId",
                table: "Diagnosticos",
                column: "TecnicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diagnosticos");
        }
    }
}
