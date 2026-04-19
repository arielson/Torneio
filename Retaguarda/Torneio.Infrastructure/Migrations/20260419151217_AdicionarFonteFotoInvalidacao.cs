using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarFonteFotoInvalidacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "fonte_foto",
                table: "capturas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "invalidada",
                table: "capturas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "motivo_invalidacao",
                table: "capturas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fonte_foto",
                table: "capturas");

            migrationBuilder.DropColumn(
                name: "invalidada",
                table: "capturas");

            migrationBuilder.DropColumn(
                name: "motivo_invalidacao",
                table: "capturas");
        }
    }
}
