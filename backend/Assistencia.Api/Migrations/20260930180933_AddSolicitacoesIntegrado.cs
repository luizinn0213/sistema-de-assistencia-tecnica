using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assistencia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitacoesIntegrado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Solicitacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    DescricaoProblema = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    EspecialidadeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Equipamentos_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalTable: "Equipamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitacoes_Especialidades_EspecialidadeId",
                        column: x => x.EspecialidadeId,
                        principalTable: "Especialidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_ClienteId",
                table: "Solicitacoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_EquipamentoId",
                table: "Solicitacoes",
                column: "EquipamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_EspecialidadeId",
                table: "Solicitacoes",
                column: "EspecialidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_Numero",
                table: "Solicitacoes",
                column: "Numero",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SelecoesTecnicos_Solicitacoes_SolicitacaoId",
                table: "SelecoesTecnicos",
                column: "SolicitacaoId",
                principalTable: "Solicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SelecoesTecnicos_Solicitacoes_SolicitacaoId",
                table: "SelecoesTecnicos");

            migrationBuilder.DropTable(
                name: "Solicitacoes");
        }
    }
}
