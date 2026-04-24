using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddValorParcelaTorneio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "valores_parcelas_torneio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_parcela = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_valores_parcelas_torneio", x => x.id);
                    table.ForeignKey(
                        name: "fk_valores_parcelas_torneio_torneio_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_valores_parcelas_torneio_torneio_id_numero_parcela",
                table: "valores_parcelas_torneio",
                columns: new[] { "torneio_id", "numero_parcela" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "valores_parcelas_torneio");
        }
    }
}
