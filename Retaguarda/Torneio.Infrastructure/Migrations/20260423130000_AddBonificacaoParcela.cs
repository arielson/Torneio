using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBonificacaoParcela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "bonificada",
                table: "parcelas_torneio",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "doacao_patrocinador_id",
                table: "parcelas_torneio",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_bonificacao",
                table: "parcelas_torneio",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bonificada",
                table: "parcelas_torneio");

            migrationBuilder.DropColumn(
                name: "doacao_patrocinador_id",
                table: "parcelas_torneio");

            migrationBuilder.DropColumn(
                name: "motivo_bonificacao",
                table: "parcelas_torneio");
        }
    }
}
