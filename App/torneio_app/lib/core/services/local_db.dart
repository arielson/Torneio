import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';
import '../models/captura.dart';
import 'package:uuid/uuid.dart';

class LocalDb {
  static Database? _db;
  static const _dbName = 'torneio_offline.db';
  static const _dbVersion = 2;

  static const _tableCapturas = 'capturas_pendentes';

  static Future<Database> get db async {
    _db ??= await _open();
    return _db!;
  }

  static Future<Database> _open() async {
    final path = join(await getDatabasesPath(), _dbName);
    return openDatabase(
      path,
      version: _dbVersion,
      onCreate: (db, version) async {
        await db.execute('''
          CREATE TABLE $_tableCapturas (
            id TEXT PRIMARY KEY,
            torneioId TEXT NOT NULL,
            itemId TEXT NOT NULL,
            membroId TEXT NOT NULL,
            equipeId TEXT NOT NULL,
            tamanhoMedida REAL NOT NULL,
            fotoUrl TEXT NOT NULL,
            dataHora TEXT NOT NULL,
            pendenteSync INTEGER NOT NULL DEFAULT 1
          )
        ''');
      },
      onUpgrade: (db, oldVersion, newVersion) async {
        // Drop and recreate to remove anoTorneioId column
        await db.execute('DROP TABLE IF EXISTS $_tableCapturas');
        await db.execute('''
          CREATE TABLE $_tableCapturas (
            id TEXT PRIMARY KEY,
            torneioId TEXT NOT NULL,
            itemId TEXT NOT NULL,
            membroId TEXT NOT NULL,
            equipeId TEXT NOT NULL,
            tamanhoMedida REAL NOT NULL,
            fotoUrl TEXT NOT NULL,
            dataHora TEXT NOT NULL,
            pendenteSync INTEGER NOT NULL DEFAULT 1
          )
        ''');
      },
    );
  }

  /// Salva uma captura pendente de sync
  static Future<String> salvarCapturaPendente(RegistrarCapturaRequest req) async {
    final database = await db;
    final id = const Uuid().v4();
    await database.insert(
      _tableCapturas,
      req.toDbMap(id),
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
    return id;
  }

  /// Lista todas as capturas com pendenteSync = 1
  static Future<List<RegistrarCapturaRequest>> listarPendentes() async {
    final database = await db;
    final rows = await database.query(
      _tableCapturas,
      where: 'pendenteSync = ?',
      whereArgs: [1],
    );
    return rows.map(_rowToRequest).toList();
  }

  /// Conta capturas pendentes
  static Future<int> contarPendentes() async {
    final database = await db;
    final result = await database.rawQuery(
      'SELECT COUNT(*) as cnt FROM $_tableCapturas WHERE pendenteSync = 1',
    );
    return (result.first['cnt'] as int?) ?? 0;
  }

  /// Remove capturas depois do sync bem-sucedido
  static Future<void> removerSincronizadas() async {
    final database = await db;
    await database.delete(_tableCapturas, where: 'pendenteSync = ?', whereArgs: [1]);
  }

  /// Remove uma captura pelo id local
  static Future<void> remover(String id) async {
    final database = await db;
    await database.delete(_tableCapturas, where: 'id = ?', whereArgs: [id]);
  }

  static RegistrarCapturaRequest _rowToRequest(Map<String, dynamic> row) {
    return RegistrarCapturaRequest(
      torneioId: row['torneioId'] as String,
      itemId: row['itemId'] as String,
      membroId: row['membroId'] as String,
      equipeId: row['equipeId'] as String,
      tamanhoMedida: (row['tamanhoMedida'] as num).toDouble(),
      fotoUrl: row['fotoUrl'] as String,
      dataHora: DateTime.parse(row['dataHora'] as String),
      pendenteSync: (row['pendenteSync'] as int) == 1,
    );
  }
}
