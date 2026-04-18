using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemComprimentoOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "comprimento",
                table: "itens",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE itens
                SET comprimento = 0
                WHERE comprimento IS NULL;
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "comprimento",
                table: "itens",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldNullable: true);
        }
    }
}
