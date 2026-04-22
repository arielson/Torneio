using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustarEquipeFinanceiroETamanhoCamisaMembro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "responsavel_financeiro",
                table: "equipes");

            migrationBuilder.AddColumn<string>(
                name: "tamanho_camisa",
                table: "membros",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tamanho_camisa",
                table: "membros");

            migrationBuilder.AddColumn<string>(
                name: "responsavel_financeiro",
                table: "equipes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
