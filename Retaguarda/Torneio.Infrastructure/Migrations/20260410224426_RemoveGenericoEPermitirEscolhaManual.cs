using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGenericoEPermitirEscolhaManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migra registros antigos com tipo Generico (0) para Pesca (1)
            migrationBuilder.Sql("UPDATE torneio SET tipo_torneio = 1 WHERE tipo_torneio = 0");

            migrationBuilder.DropColumn(
                name: "permitir_escolha_manual",
                table: "torneio");

            migrationBuilder.AlterColumn<int>(
                name: "tipo_torneio",
                table: "torneio",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "tipo_torneio",
                table: "torneio",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "permitir_escolha_manual",
                table: "torneio",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
