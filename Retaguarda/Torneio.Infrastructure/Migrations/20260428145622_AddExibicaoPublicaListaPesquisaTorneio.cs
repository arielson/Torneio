using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExibicaoPublicaListaPesquisaTorneio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "exibir_na_lista_inicial_publica",
                table: "torneio",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "exibir_na_pesquisa_publica",
                table: "torneio",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exibir_na_lista_inicial_publica",
                table: "torneio");

            migrationBuilder.DropColumn(
                name: "exibir_na_pesquisa_publica",
                table: "torneio");
        }
    }
}
