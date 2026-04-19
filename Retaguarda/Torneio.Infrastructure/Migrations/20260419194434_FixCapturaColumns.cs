using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Torneio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCapturaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Garante colunas que migrações manuais anteriores deixaram de criar
            migrationBuilder.Sql(@"
                ALTER TABLE capturas ADD COLUMN IF NOT EXISTS origem integer NOT NULL DEFAULT 0;
                ALTER TABLE capturas ADD COLUMN IF NOT EXISTS fonte_foto integer NULL;
                ALTER TABLE capturas ADD COLUMN IF NOT EXISTS invalidada boolean NOT NULL DEFAULT false;
                ALTER TABLE capturas ADD COLUMN IF NOT EXISTS motivo_invalidacao character varying(500) NULL;
                ALTER TABLE capturas ALTER COLUMN foto_url DROP NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE capturas DROP COLUMN IF EXISTS origem;
                ALTER TABLE capturas DROP COLUMN IF EXISTS fonte_foto;
                ALTER TABLE capturas DROP COLUMN IF EXISTS invalidada;
                ALTER TABLE capturas DROP COLUMN IF EXISTS motivo_invalidacao;
            ");
        }
    }
}
