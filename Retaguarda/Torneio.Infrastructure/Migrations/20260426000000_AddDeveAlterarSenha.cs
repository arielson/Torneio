using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeveAlterarSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "deve_alterar_senha",
                table: "admins_torneio",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "deve_alterar_senha",
                table: "fiscais",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "deve_alterar_senha",
                table: "membros",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "deve_alterar_senha", table: "admins_torneio");
            migrationBuilder.DropColumn(name: "deve_alterar_senha", table: "fiscais");
            migrationBuilder.DropColumn(name: "deve_alterar_senha", table: "membros");
        }
    }
}
