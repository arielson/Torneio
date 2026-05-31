using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Asaas_IntegraçãoPagamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cpf",
                table: "membros",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cobrancas_asaas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parcela_torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asaas_payment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    asaas_customer_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    asaas_invoice_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    valor_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxa_asaas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_previsao_credito = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_credito_efetivo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cobrancas_asaas", x => x.id);
                    table.ForeignKey(
                        name: "fk_cobrancas_asaas_membros_membro_id",
                        column: x => x.membro_id,
                        principalTable: "membros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cobrancas_asaas_parcelas_torneio_parcela_torneio_id",
                        column: x => x.parcela_torneio_id,
                        principalTable: "parcelas_torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cobrancas_asaas_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_asaas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave_api_asaas = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status_chave = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    asaas_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    aceitar_pix = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    aceitar_cartao_credito = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_ativacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configuracoes_asaas", x => x.id);
                    table.ForeignKey(
                        name: "fk_configuracoes_asaas_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webhook_eventos_asaas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    asaas_payment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    processado = table.Column<bool>(type: "boolean", nullable: false),
                    erro_processamento = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    recebido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_eventos_asaas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cobrancas_asaas_asaas_payment_id",
                table: "cobrancas_asaas",
                column: "asaas_payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cobrancas_asaas_membro_id",
                table: "cobrancas_asaas",
                column: "membro_id");

            migrationBuilder.CreateIndex(
                name: "ix_cobrancas_asaas_parcela_torneio_id",
                table: "cobrancas_asaas",
                column: "parcela_torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_cobrancas_asaas_torneio_id_membro_id",
                table: "cobrancas_asaas",
                columns: new[] { "torneio_id", "membro_id" });

            migrationBuilder.CreateIndex(
                name: "ix_configuracoes_asaas_torneio_id",
                table: "configuracoes_asaas",
                column: "torneio_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_eventos_asaas_evento_id",
                table: "webhook_eventos_asaas",
                column: "evento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_eventos_asaas_processado_recebido_em",
                table: "webhook_eventos_asaas",
                columns: new[] { "processado", "recebido_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cobrancas_asaas");

            migrationBuilder.DropTable(
                name: "configuracoes_asaas");

            migrationBuilder.DropTable(
                name: "webhook_eventos_asaas");

            migrationBuilder.DropColumn(
                name: "cpf",
                table: "membros");
        }
    }
}
