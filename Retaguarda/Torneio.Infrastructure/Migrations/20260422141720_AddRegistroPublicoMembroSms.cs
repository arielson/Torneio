using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistroPublicoMembroSms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "permitir_registro_publico_membro",
                table: "torneio",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "registros_publicos_membros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    celular = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    celular_normalizado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tamanho_camisa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    quantidade_envios = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tentativas_validacao = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ultimo_envio_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    membro_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registros_publicos_membros", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_registros_publicos_membros_torneio_id_celular_normalizado_c",
                table: "registros_publicos_membros",
                columns: new[] { "torneio_id", "celular_normalizado", "criado_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registros_publicos_membros");

            migrationBuilder.DropColumn(
                name: "permitir_registro_publico_membro",
                table: "torneio");
        }
    }
}
