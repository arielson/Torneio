using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceiroOperacionalTorneio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_primeiro_vencimento",
                table: "torneio",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantidade_parcelas",
                table: "torneio",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_por_membro",
                table: "torneio",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "custo",
                table: "equipes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "responsavel_financeiro",
                table: "equipes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status_financeiro",
                table: "equipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "checklist_torneio_itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    responsavel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    concluido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checklist_torneio_itens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custos_torneio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    responsavel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custos_torneio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parcelas_torneio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_parcela = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vencimento_editado_manual = table.Column<bool>(type: "boolean", nullable: false),
                    pago = table.Column<bool>(type: "boolean", nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    comprovante_nome_arquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    comprovante_data_upload = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    comprovante_usuario_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    comprovante_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    comprovante_content_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parcelas_torneio", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_torneio_torneio_id_membro_id_numero_parcela",
                table: "parcelas_torneio",
                columns: new[] { "torneio_id", "membro_id", "numero_parcela" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checklist_torneio_itens");

            migrationBuilder.DropTable(
                name: "custos_torneio");

            migrationBuilder.DropTable(
                name: "parcelas_torneio");

            migrationBuilder.DropColumn(
                name: "data_primeiro_vencimento",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "quantidade_parcelas",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "valor_por_membro",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "custo",
                table: "equipes");

            migrationBuilder.DropColumn(
                name: "responsavel_financeiro",
                table: "equipes");

            migrationBuilder.DropColumn(
                name: "status_financeiro",
                table: "equipes");
        }
    }
}
