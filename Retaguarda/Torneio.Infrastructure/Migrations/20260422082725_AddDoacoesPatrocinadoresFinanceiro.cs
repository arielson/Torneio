using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoacoesPatrocinadoresFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "doacoes_patrocinadores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patrocinador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_patrocinador = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: true),
                    valor = table.Column<decimal>(type: "numeric", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    data_doacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_doacoes_patrocinadores", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doacoes_patrocinadores");
        }
    }
}
