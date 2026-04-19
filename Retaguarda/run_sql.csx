// Script temporário — remover após uso
using var conn = new Npgsql.NpgsqlConnection("Host=localhost;Database=torneio;Username=postgres;Password=Httpr0x1!");
await conn.OpenAsync();

var sql = @"
CREATE TABLE IF NOT EXISTS logs_auditoria (
    id uuid PRIMARY KEY,
    torneio_id uuid,
    nome_torneio varchar(200),
    categoria varchar(50) NOT NULL,
    acao varchar(100) NOT NULL,
    descricao varchar(1000) NOT NULL,
    usuario_nome varchar(200) NOT NULL,
    usuario_perfil varchar(50) NOT NULL,
    ip_address varchar(50),
    data_hora timestamptz NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_logs_auditoria_data_hora ON logs_auditoria(data_hora);
CREATE INDEX IF NOT EXISTS ix_logs_auditoria_torneio_id ON logs_auditoria(torneio_id);
CREATE INDEX IF NOT EXISTS ix_logs_auditoria_categoria ON logs_auditoria(categoria);
-- Garante que __EFMigrationsHistory registre a migration (já deve estar, mas por garantia)
INSERT INTO ""__EFMigrationsHistory""(""MigrationId"", ""ProductVersion"")
VALUES ('20260419152433_AdicionarLogAuditoria', '9.0.1')
ON CONFLICT DO NOTHING;
";

using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
await cmd.ExecuteNonQueryAsync();
Console.WriteLine("OK — tabela logs_auditoria criada/confirmada.");
