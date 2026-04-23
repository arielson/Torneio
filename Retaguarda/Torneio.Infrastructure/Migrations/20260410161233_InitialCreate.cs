using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins_geral",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admins_geral", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "torneio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome_torneio = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    label_equipe = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label_membro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label_supervisor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label_item = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label_captura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    usar_fator_multiplicador = table.Column<bool>(type: "boolean", nullable: false),
                    medida_captura = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    permitir_captura_offline = table.Column<bool>(type: "boolean", nullable: false),
                    modo_sorteio = table.Column<int>(type: "integer", nullable: false),
                    permitir_escolha_manual = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_torneio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admins_torneio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admins_torneio", x => x.id);
                    table.ForeignKey(
                        name: "fk_admins_torneio_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    comprimento = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    fator_multiplicador = table.Column<decimal>(type: "numeric(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_itens_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fiscais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fiscais", x => x.id);
                    table.ForeignKey(
                        name: "fk_fiscais_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "membros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membros", x => x.id);
                    table.ForeignKey(
                        name: "fk_membros_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    capitao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    foto_capitao_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    qtd_vagas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipes", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipes_fiscais_fiscal_id",
                        column: x => x.fiscal_id,
                        principalTable: "fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_equipes_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "capturas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tamanho_medida = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pendente_sync = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_capturas", x => x.id);
                    table.ForeignKey(
                        name: "fk_capturas_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_capturas_itens_item_id",
                        column: x => x.item_id,
                        principalTable: "itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_capturas_membros_membro_id",
                        column: x => x.membro_id,
                        principalTable: "membros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_capturas_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipe_membro",
                columns: table => new
                {
                    equipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membros_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipe_membro", x => new { x.equipe_id, x.membros_id });
                    table.ForeignKey(
                        name: "fk_equipe_membro_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_equipe_membro_membros_membros_id",
                        column: x => x.membros_id,
                        principalTable: "membros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sorteios_equipe",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    torneio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posicao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sorteios_equipe", x => x.id);
                    table.ForeignKey(
                        name: "fk_sorteios_equipe_equipes_equipe_id",
                        column: x => x.equipe_id,
                        principalTable: "equipes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sorteios_equipe_membros_membro_id",
                        column: x => x.membro_id,
                        principalTable: "membros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sorteios_equipe_torneiros_torneio_id",
                        column: x => x.torneio_id,
                        principalTable: "torneio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admins_geral_usuario",
                table: "admins_geral",
                column: "usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_admins_torneio_torneio_id",
                table: "admins_torneio",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_admins_torneio_usuario_torneio_id",
                table: "admins_torneio",
                columns: new[] { "usuario", "torneio_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_capturas_equipe_id",
                table: "capturas",
                column: "equipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_capturas_item_id",
                table: "capturas",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_capturas_membro_id",
                table: "capturas",
                column: "membro_id");

            migrationBuilder.CreateIndex(
                name: "ix_capturas_pendente_sync",
                table: "capturas",
                column: "pendente_sync");

            migrationBuilder.CreateIndex(
                name: "ix_capturas_torneio_id_equipe_id",
                table: "capturas",
                columns: new[] { "torneio_id", "equipe_id" });

            migrationBuilder.CreateIndex(
                name: "ix_capturas_torneio_id_membro_id",
                table: "capturas",
                columns: new[] { "torneio_id", "membro_id" });

            migrationBuilder.CreateIndex(
                name: "ix_equipe_membro_membros_id",
                table: "equipe_membro",
                column: "membros_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipes_fiscal_id",
                table: "equipes",
                column: "fiscal_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipes_torneio_id",
                table: "equipes",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_fiscais_torneio_id",
                table: "fiscais",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_fiscais_usuario_torneio_id",
                table: "fiscais",
                columns: new[] { "usuario", "torneio_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_itens_torneio_id",
                table: "itens",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_membros_torneio_id",
                table: "membros",
                column: "torneio_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_equipe_id",
                table: "sorteios_equipe",
                column: "equipe_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_membro_id",
                table: "sorteios_equipe",
                column: "membro_id");

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_torneio_id_equipe_id",
                table: "sorteios_equipe",
                columns: new[] { "torneio_id", "equipe_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sorteios_equipe_torneio_id_posicao",
                table: "sorteios_equipe",
                columns: new[] { "torneio_id", "posicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_torneio_slug",
                table: "torneio",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins_geral");

            migrationBuilder.DropTable(
                name: "admins_torneio");

            migrationBuilder.DropTable(
                name: "capturas");

            migrationBuilder.DropTable(
                name: "equipe_membro");

            migrationBuilder.DropTable(
                name: "sorteios_equipe");

            migrationBuilder.DropTable(
                name: "itens");

            migrationBuilder.DropTable(
                name: "equipes");

            migrationBuilder.DropTable(
                name: "membros");

            migrationBuilder.DropTable(
                name: "fiscais");

            migrationBuilder.DropTable(
                name: "torneio");
        }
    }
}
