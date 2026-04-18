using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoverAnoTorneio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_capturas_anos_torneio_ano_torneio_id",
                table: "capturas");

            migrationBuilder.DropForeignKey(
                name: "fk_equipes_anos_torneio_ano_torneio_id",
                table: "equipes");

            migrationBuilder.DropForeignKey(
                name: "fk_fiscais_anos_torneio_ano_torneio_id",
                table: "fiscais");

            migrationBuilder.DropForeignKey(
                name: "fk_membros_anos_torneio_ano_torneio_id",
                table: "membros");

            migrationBuilder.DropForeignKey(
                name: "fk_premios_anos_torneio_ano_torneio_id",
                table: "premios");

            migrationBuilder.DropForeignKey(
                name: "fk_sorteios_equipe_anos_torneio_ano_torneio_id",
                table: "sorteios_equipe");

            migrationBuilder.DropTable(
                name: "anos_torneio");

            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_ano_torneio_id_equipe_id",
                table: "sorteios_equipe");

            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_ano_torneio_id_posicao",
                table: "sorteios_equipe");

            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_torneio_id",
                table: "sorteios_equipe");

            migrationBuilder.DropIndex(
                name: "ix_premios_ano_torneio_id_posicao",
                table: "premios");

            migrationBuilder.DropIndex(
                name: "ix_premios_torneio_id",
                table: "premios");

            migrationBuilder.DropIndex(
                name: "ix_membros_ano_torneio_id",
                table: "membros");

            migrationBuilder.DropIndex(
                name: "ix_fiscais_ano_torneio_id",
                table: "fiscais");

            migrationBuilder.DropIndex(
                name: "ix_equipes_ano_torneio_id",
                table: "equipes");

            migrationBuilder.DropIndex(
                name: "ix_capturas_ano_torneio_id_equipe_id",
                table: "capturas");

            migrationBuilder.DropIndex(
                name: "ix_capturas_ano_torneio_id_membro_id",
                table: "capturas");

            migrationBuilder.DropIndex(
                name: "ix_capturas_torneio_id",
                table: "capturas");

            migrationBuilder.DropColumn(
                name: "ano_torneio_id",
                table: "sorteios_equipe");

            migrationBuilder.DropColumn(
                name: "ano_torneio_id",
                table: "premios");

            migrationBuilder.DropColumn(
                name: "ano_torneio_id",
                table: "membros");

            migrationBuilder.DropColumn(
                name: "ano_torneio_id",
                table: "fiscais");

            migrationBuilder.DropColumn(
                name: "ano_torneio_id",
                table: "equipes");

            migrationBuilder.DropColumn(
                name: "ano_torneio_id",
                table: "capturas");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "torneio",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_torneio_id_equipe_id",
                table: "sorteios_equipe",
                columns: new[] { "torneio_id", "equipe_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_torneio_id_posicao",
                table: "sorteios_equipe",
                columns: new[] { "torneio_id", "posicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_premios_torneio_id_posicao",
                table: "premios",
                columns: new[] { "torneio_id", "posicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_capturas_torneio_id_equipe_id",
                table: "capturas",
                columns: new[] { "torneio_id", "equipe_id" });

            migrationBuilder.CreateIndex(
                name: "ix_capturas_torneio_id_membro_id",
                table: "capturas",
                columns: new[] { "torneio_id", "membro_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_torneio_id_equipe_id",
                table: "sorteios_equipe");

            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_torneio_id_posicao",
                table: "sorteios_equipe");

            migrationBuilder.DropIndex(
                name: "ix_premios_torneio_id_posicao",
                table: "premios");

            migrationBuilder.DropIndex(
                name: "ix_capturas_torneio_id_equipe_id",
                table: "capturas");

            migrationBuilder.DropIndex(
                name: "ix_capturas_torneio_id_membro_id",
                table: "capturas");

            migrationBuilder.DropColumn(
                name: "status",
                table: "torneio");

            migrationBuilder.AddColumn<Guid>(
                name: "ano_torneio_id",
                table: "sorteios_equipe",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ano_torneio_id",
                table: "premios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ano_torneio_id",
                table: "membros",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ano_torneio_id",
                table: "fiscais",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ano_torneio_id",
                table: "equipes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ano_torneio_id",
                table: "capturas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "anos_torneio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anos_torneio", x => x.id);
                    table.ForeignKey(
                        name: "fk_anos_torneio_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_ano_torneio_id_equipe_id",
                table: "sorteios_equipe",
                columns: new[] { "ano_torneio_id", "equipe_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_ano_torneio_id_posicao",
                table: "sorteios_equipe",
                columns: new[] { "ano_torneio_id", "posicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_torneio_id",
                table: "sorteios_equipe",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_premios_ano_torneio_id_posicao",
                table: "premios",
                columns: new[] { "ano_torneio_id", "posicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_premios_torneio_id",
                table: "premios",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_membros_ano_torneio_id",
                table: "membros",
                column: "ano_torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_fiscais_ano_torneio_id",
                table: "fiscais",
                column: "ano_torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipes_ano_torneio_id",
                table: "equipes",
                column: "ano_torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_capturas_ano_torneio_id_equipe_id",
                table: "capturas",
                columns: new[] { "ano_torneio_id", "equipe_id" });

            migrationBuilder.CreateIndex(
                name: "ix_capturas_ano_torneio_id_membro_id",
                table: "capturas",
                columns: new[] { "ano_torneio_id", "membro_id" });

            migrationBuilder.CreateIndex(
                name: "ix_capturas_torneio_id",
                table: "capturas",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_anos_torneio_torneio_id_titulo",
                table: "anos_torneio",
                columns: new[] { "torneio_id", "titulo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_capturas_anos_torneio_ano_torneio_id",
                table: "capturas",
                column: "ano_torneio_id",
                principalTable: "anos_torneio",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_equipes_anos_torneio_ano_torneio_id",
                table: "equipes",
                column: "ano_torneio_id",
                principalTable: "anos_torneio",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_fiscais_anos_torneio_ano_torneio_id",
                table: "fiscais",
                column: "ano_torneio_id",
                principalTable: "anos_torneio",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_membros_anos_torneio_ano_torneio_id",
                table: "membros",
                column: "ano_torneio_id",
                principalTable: "anos_torneio",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_premios_anos_torneio_ano_torneio_id",
                table: "premios",
                column: "ano_torneio_id",
                principalTable: "anos_torneio",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_sorteios_equipe_anos_torneio_ano_torneio_id",
                table: "sorteios_equipe",
                column: "ano_torneio_id",
                principalTable: "anos_torneio",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
