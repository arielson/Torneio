using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinanceiroTaxaInscricaoEExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_parcelas_torneio_torneio_id_membro_id_numero_parcela",
                table: "parcelas_torneio");

            migrationBuilder.AddColumn<DateTime>(
                name: "data_vencimento_taxa_inscricao",
                table: "torneio",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "taxa_inscricao_valor",
                table: "torneio",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "parcelas_torneio",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "referencia_id",
                table: "parcelas_torneio",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_parcela",
                table: "parcelas_torneio",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "produtos_extras_membros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_extra_torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_cobrado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos_extras_membros", x => x.id);
                    table.ForeignKey(
                        name: "fk_produtos_extras_membros_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produtos_extras_torneio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos_extras_torneio", x => x.id);
                    table.ForeignKey(
                        name: "fk_produtos_extras_torneio_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_torneio_torneio_id_membro_id_tipo_parcela_numero_p",
                table: "parcelas_torneio",
                columns: new[] { "torneio_id", "membro_id", "tipo_parcela", "numero_parcela", "referencia_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produtos_extras_membros_produto_extra_torneio_id_membro_id",
                table: "produtos_extras_membros",
                columns: new[] { "produto_extra_torneio_id", "membro_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produtos_extras_membros_torneio_id",
                table: "produtos_extras_membros",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_extras_torneio_torneio_id",
                table: "produtos_extras_torneio",
                column: "torneio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produtos_extras_membros");

            migrationBuilder.DropTable(
                name: "produtos_extras_torneio");

            migrationBuilder.DropIndex(
                name: "ix_parcelas_torneio_torneio_id_membro_id_tipo_parcela_numero_p",
                table: "parcelas_torneio");

            migrationBuilder.DropColumn(
                name: "data_vencimento_taxa_inscricao",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "taxa_inscricao_valor",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "parcelas_torneio");

            migrationBuilder.DropColumn(
                name: "referencia_id",
                table: "parcelas_torneio");

            migrationBuilder.DropColumn(
                name: "tipo_parcela",
                table: "parcelas_torneio");

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_torneio_torneio_id_membro_id_numero_parcela",
                table: "parcelas_torneio",
                columns: new[] { "torneio_id", "membro_id", "numero_parcela" },
                unique: true);
        }
    }
}
