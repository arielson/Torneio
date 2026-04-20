using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGrupoEquipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grupos", x => x.id);
                    table.ForeignKey(
                        name: "fk_grupos_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grupos_membros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membro_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grupos_membros", x => x.id);
                    table.ForeignKey(
                        name: "fk_grupos_membros_grupos_grupo_id",
                        column: x => x.grupo_id,
                        principalTable: "grupos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grupos_membros_membros_membro_id",
                        column: x => x.membro_id,
                        principalTable: "membros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sorteios_grupo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posicao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sorteios_grupo", x => x.id);
                    table.ForeignKey(
                        name: "fk_sorteios_grupo_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sorteios_grupo_grupos_grupo_id",
                        column: x => x.grupo_id,
                        principalTable: "grupos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sorteios_grupo_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_grupos_torneio_id",
                table: "grupos",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_grupos_torneio_id_nome",
                table: "grupos",
                columns: new[] { "torneio_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_grupos_membros_grupo_id_membro_id",
                table: "grupos_membros",
                columns: new[] { "grupo_id", "membro_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_grupos_membros_membro_id",
                table: "grupos_membros",
                column: "membro_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_grupo_equipe_id",
                table: "sorteios_grupo",
                column: "equipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_grupo_grupo_id",
                table: "sorteios_grupo",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_grupo_torneio_id",
                table: "sorteios_grupo",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_grupo_torneio_id_equipe_id",
                table: "sorteios_grupo",
                columns: new[] { "torneio_id", "equipe_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_grupo_torneio_id_grupo_id",
                table: "sorteios_grupo",
                columns: new[] { "torneio_id", "grupo_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grupos_membros");

            migrationBuilder.DropTable(
                name: "sorteios_grupo");

            migrationBuilder.DropTable(
                name: "grupos");
        }
    }
}
