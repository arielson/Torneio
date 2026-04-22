using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCredenciaisMembro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_membros_torneio_id",
                table: "membros");

            migrationBuilder.AddColumn<string>(
                name: "senha_hash",
                table: "membros",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "usuario",
                table: "membros",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_membros_torneio_id_usuario",
                table: "membros",
                columns: new[] { "torneio_id", "usuario" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_membros_torneio_id_usuario",
                table: "membros");

            migrationBuilder.DropColumn(
                name: "senha_hash",
                table: "membros");

            migrationBuilder.DropColumn(
                name: "usuario",
                table: "membros");

            migrationBuilder.CreateIndex(
                name: "ix_membros_torneio_id",
                table: "membros",
                column: "torneio_id");
        }
    }
}
