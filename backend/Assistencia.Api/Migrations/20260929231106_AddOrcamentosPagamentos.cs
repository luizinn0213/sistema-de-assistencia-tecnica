using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assistencia.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrcamentosPagamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orcamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiagnosticoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiagnosticoConcluido = table.Column<bool>(type: "INTEGER", nullable: false),
                    Versao = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orcamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrcamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Porcentagem = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorComissao = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataCalculo = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comissoes_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosOrcamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrcamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Versao = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Obs = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosOrcamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosOrcamento_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensOrcamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrcamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensOrcamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensOrcamento_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrcamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", nullable: false),
                    FormaPagamento = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comissoes_OrcamentoId",
                table: "Comissoes",
                column: "OrcamentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosOrcamento_OrcamentoId",
                table: "HistoricosOrcamento",
                column: "OrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrcamento_OrcamentoId",
                table: "ItensOrcamento",
                column: "OrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_OrcamentoId",
                table: "Pagamentos",
                column: "OrcamentoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comissoes");

            migrationBuilder.DropTable(
                name: "HistoricosOrcamento");

            migrationBuilder.DropTable(
                name: "ItensOrcamento");

            migrationBuilder.DropTable(
                name: "Pagamentos");

            migrationBuilder.DropTable(
                name: "Orcamentos");
        }
    }
}
