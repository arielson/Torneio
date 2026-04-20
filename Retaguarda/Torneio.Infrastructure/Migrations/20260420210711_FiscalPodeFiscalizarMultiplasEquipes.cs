using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FiscalPodeFiscalizarMultiplasEquipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fiscal_equipe",
                columns: table => new
                {
                    fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipe_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fiscal_equipe", x => new { x.fiscal_id, x.equipe_id });
                    table.ForeignKey(
                        name: "fk_fiscal_equipe_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_fiscal_equipe_fiscais_fiscal_id",
                        column: x => x.fiscal_id,
                        principalTable: "fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_equipe_equipe_id",
                table: "fiscal_equipe",
                column: "equipe_id");

            migrationBuilder.Sql("""
                INSERT INTO fiscal_equipe (fiscal_id, equipe_id)
                SELECT fiscal_id, id
                FROM equipes
                WHERE fiscal_id IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_equipes_fiscais_fiscal_id",
                table: "equipes");

            migrationBuilder.DropIndex(
                name: "ix_equipes_fiscal_id",
                table: "equipes");

            migrationBuilder.DropColumn(
                name: "fiscal_id",
                table: "equipes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fiscal_equipe");

            migrationBuilder.AddColumn<Guid>(
                name: "fiscal_id",
                table: "equipes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_equipes_fiscal_id",
                table: "equipes",
                column: "fiscal_id");

            migrationBuilder.AddForeignKey(
                name: "fk_equipes_fiscais_fiscal_id",
                table: "equipes",
                column: "fiscal_id",
                principalTable: "fiscais",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
