using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assistencia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExecucoesAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecucoesServicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrcamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ObservacaoInicio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ObservacaoFinal = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucoesServicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecucoesServicos_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExecucaoServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nota = table.Column<int>(type: "INTEGER", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataAvaliacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_ExecucoesServicos_ExecucaoServicoId",
                        column: x => x.ExecucaoServicoId,
                        principalTable: "ExecucoesServicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosExecucoesServicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExecucaoServicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosExecucoesServicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosExecucoesServicos_ExecucoesServicos_ExecucaoServicoId",
                        column: x => x.ExecucaoServicoId,
                        principalTable: "ExecucoesServicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ClienteId",
                table: "Avaliacoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ExecucaoServicoId",
                table: "Avaliacoes",
                column: "ExecucaoServicoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExecucoesServicos_OrcamentoId",
                table: "ExecucoesServicos",
                column: "OrcamentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosExecucoesServicos_ExecucaoServicoId_DataRegistro",
                table: "HistoricosExecucoesServicos",
                columns: new[] { "ExecucaoServicoId", "DataRegistro" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "HistoricosExecucoesServicos");

            migrationBuilder.DropTable(
                name: "ExecucoesServicos");
        }
    }
}
