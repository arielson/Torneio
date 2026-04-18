using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLabelsPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "label_captura_plural",
                table: "torneio",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "label_equipe_plural",
                table: "torneio",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "label_item_plural",
                table: "torneio",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "label_membro_plural",
                table: "torneio",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "label_supervisor_plural",
                table: "torneio",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "label_captura_plural",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "label_equipe_plural",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "label_item_plural",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "label_membro_plural",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "label_supervisor_plural",
                table: "torneio");
        }
    }
}
