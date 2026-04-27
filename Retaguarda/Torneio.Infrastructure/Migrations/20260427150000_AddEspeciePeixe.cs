using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEspeciePeixe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Criar tabela global de espécies de peixe
            migrationBuilder.CreateTable(
                name: "especies_peixe",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nome_cientifico = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_especies_peixe", x => x.id);
                });

            // 2. Garantir extensão de UUID disponível e migrar itens existentes
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS ""pgcrypto"";");

            migrationBuilder.Sql(@"
                INSERT INTO especies_peixe (id, nome, nome_cientifico, foto_url)
                SELECT gen_random_uuid(), nome, NULL, MAX(foto_url)
                FROM itens
                GROUP BY nome;
            ");

            // 3. Adicionar coluna FK (nullable primeiro para preencher)
            migrationBuilder.AddColumn<Guid>(
                name: "especie_peixe_id",
                table: "itens",
                type: "uuid",
                nullable: true);

            // 4. Preencher FK com base no nome
            migrationBuilder.Sql(@"
                UPDATE itens SET especie_peixe_id = ep.id
                FROM especies_peixe ep
                WHERE itens.nome = ep.nome;
            ");

            // 5. Tornar NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "especie_peixe_id",
                table: "itens",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // 6. Adicionar FK constraint
            migrationBuilder.AddForeignKey(
                name: "fk_itens_especies_peixe_especie_peixe_id",
                table: "itens",
                column: "especie_peixe_id",
                principalTable: "especies_peixe",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // 7. Remover colunas obsoletas de itens
            migrationBuilder.DropColumn(name: "nome", table: "itens");
            migrationBuilder.DropColumn(name: "foto_url", table: "itens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_itens_especies_peixe_especie_peixe_id",
                table: "itens");

            // Restaurar colunas em itens
            migrationBuilder.AddColumn<string>(
                name: "nome",
                table: "itens",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "foto_url",
                table: "itens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            // Restaurar nome/foto a partir da espécie vinculada
            migrationBuilder.Sql(@"
                UPDATE itens SET
                    nome = ep.nome,
                    foto_url = ep.foto_url
                FROM especies_peixe ep
                WHERE itens.especie_peixe_id = ep.id;
            ");

            migrationBuilder.DropColumn(name: "especie_peixe_id", table: "itens");
            migrationBuilder.DropTable(name: "especies_peixe");
        }
    }
}
