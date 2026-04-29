using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDatasAdminTorneioAcessoTrocaSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "senha_alterada_em",
                table: "admins_torneio",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ultimo_acesso_em",
                table: "admins_torneio",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "senha_alterada_em",
                table: "admins_torneio");

            migrationBuilder.DropColumn(
                name: "ultimo_acesso_em",
                table: "admins_torneio");
        }
    }
}
