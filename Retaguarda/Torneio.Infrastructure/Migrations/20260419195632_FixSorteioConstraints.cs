using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSorteioConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_torneio_id_equipe_id",
                table: "sorteios_equipe");

            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_torneio_id_posicao",
                table: "sorteios_equipe");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_torneio_id",
                table: "sorteios_equipe",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_torneio_id_membro_id",
                table: "sorteios_equipe",
                columns: new[] { "torneio_id", "membro_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_torneio_id",
                table: "sorteios_equipe");

            migrationBuilder.DropIndex(
                name: "ix_sorteios_equipe_torneio_id_membro_id",
                table: "sorteios_equipe");

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
        }
    }
}
