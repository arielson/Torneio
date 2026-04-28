using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposDataDescricaoObservacoesTorneio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_torneio",
                table: "torneio",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "torneio",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observacoes_internas",
                table: "torneio",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data_torneio",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "observacoes_internas",
                table: "torneio");
        }
    }
}
