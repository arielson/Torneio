using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PescaRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_anos_torneio_torneio_id_ano",
                table: "anos_torneio");

            migrationBuilder.DropColumn(
                name: "ano",
                table: "anos_torneio");

            migrationBuilder.AddColumn<int>(
                name: "qtd_ganhadores",
                table: "torneio",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<string>(
                name: "titulo",
                table: "anos_torneio",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "premios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ano_torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posicao = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_premios", x => x.id);
                    table.ForeignKey(
                        name: "fk_premios_anos_torneio_ano_torneio_id",
                        column: x => x.ano_torneio_id,
                        principalTable: "anos_torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_premios_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_anos_torneio_torneio_id_titulo",
                table: "anos_torneio",
                columns: new[] { "torneio_id", "titulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_premios_ano_torneio_id_posicao",
                table: "premios",
                columns: new[] { "ano_torneio_id", "posicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_premios_torneio_id",
                table: "premios",
                column: "torneio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "premios");

            migrationBuilder.DropIndex(
                name: "ix_anos_torneio_torneio_id_titulo",
                table: "anos_torneio");

            migrationBuilder.DropColumn(
                name: "qtd_ganhadores",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "titulo",
                table: "anos_torneio");

            migrationBuilder.AddColumn<int>(
                name: "ano",
                table: "anos_torneio",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_anos_torneio_torneio_id_ano",
                table: "anos_torneio",
                columns: new[] { "torneio_id", "ano" },
                unique: true);
        }
    }
}
