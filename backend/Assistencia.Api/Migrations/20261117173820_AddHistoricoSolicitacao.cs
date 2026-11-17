using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assistencia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricoSolicitacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoricosSolicitacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SolicitacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusAnterior = table.Column<int>(type: "INTEGER", nullable: false),
                    NovoStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosSolicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosSolicitacoes_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosSolicitacoes_SolicitacaoId_DataAlteracao",
                table: "HistoricosSolicitacoes",
                columns: new[] { "SolicitacaoId", "DataAlteracao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricosSolicitacoes");
        }
    }
}
