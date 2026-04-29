import 'dart:convert';
import 'package:path/path.dart';
import 'package:sqflite/sqflite.dart';
import 'package:uuid/uuid.dart';
import '../models/captura.dart';

class CapturaPendenteLocal {
  final String id;
  final RegistrarCapturaRequest request;
  final bool sincronizacaoManual;

  const CapturaPendenteLocal({
    required this.id,
    required this.request,
    required this.sincronizacaoManual,
  });
}

class LocalDb {
  static Database? _db;
  static const _dbName = 'torneio_offline.db';
  static const _dbVersion = 5;

  static const _tableCapturas = 'capturas_pendentes';
  static const _tableCatalogos = 'catalogos_cache';

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
        await _criarTabelas(db);
      },
      onUpgrade: (db, oldVersion, newVersion) async {
        if (oldVersion < 3) {
          await db.execute('DROP TABLE IF EXISTS $_tableCapturas');
          await _criarTabelaCapturas(db);
        }
        if (oldVersion < 4) {
          await _criarTabelaCatalogos(db);
        }
        if (oldVersion < 5) {
          await db.execute(
            'ALTER TABLE $_tableCapturas ADD COLUMN fonteFoto INTEGER NULL',
          );
        }
      },
    );
  }

  static Future<void> _criarTabelas(Database db) async {
    await _criarTabelaCapturas(db);
    await _criarTabelaCatalogos(db);
  }

  static Future<void> _criarTabelaCapturas(Database db) async {
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
        pendenteSync INTEGER NOT NULL DEFAULT 1,
        fonteFoto INTEGER NULL,
        sincronizacaoManual INTEGER NOT NULL DEFAULT 0
      )
    ''');
  }

  static Future<void> _criarTabelaCatalogos(Database db) async {
    await db.execute('''
      CREATE TABLE IF NOT EXISTS $_tableCatalogos (
        slug TEXT NOT NULL,
        tipo TEXT NOT NULL,
        dados TEXT NOT NULL,
        atualizadoEm TEXT NOT NULL,
        PRIMARY KEY (slug, tipo)
      )
    ''');
  }

  static Future<String> salvarCapturaPendente(
    RegistrarCapturaRequest req, {
    bool sincronizacaoManual = false,
  }) async {
    final database = await db;
    await _garantirColunaFonteFoto(database);
    final id = const Uuid().v4();
    await database.insert(_tableCapturas, {
      ...req.toDbMap(id),
      'sincronizacaoManual': sincronizacaoManual ? 1 : 0,
    }, conflictAlgorithm: ConflictAlgorithm.replace);
    return id;
  }

  static Future<List<CapturaPendenteLocal>> listarPendentes({
    bool? sincronizacaoManual,
  }) async {
    final database = await db;
    await _garantirColunaFonteFoto(database);
    final where = <String>['pendenteSync = ?'];
    final args = <Object>[1];

    if (sincronizacaoManual != null) {
      where.add('sincronizacaoManual = ?');
      args.add(sincronizacaoManual ? 1 : 0);
    }

    final rows = await database.query(
      _tableCapturas,
      where: where.join(' AND '),
      whereArgs: args,
      orderBy: 'dataHora ASC',
    );

    return rows
        .map(
          (row) => CapturaPendenteLocal(
            id: row['id'] as String,
            request: _rowToRequest(row),
            sincronizacaoManual: (row['sincronizacaoManual'] as int? ?? 0) == 1,
          ),
        )
        .toList();
  }

  static Future<int> contarPendentes({bool? sincronizacaoManual}) async {
    final database = await db;
    await _garantirColunaFonteFoto(database);
    final where = <String>['pendenteSync = 1'];

    if (sincronizacaoManual != null) {
      where.add('sincronizacaoManual = ${sincronizacaoManual ? 1 : 0}');
    }

    final result = await database.rawQuery(
      'SELECT COUNT(*) as cnt FROM $_tableCapturas WHERE ${where.join(' AND ')}',
    );
    return (result.first['cnt'] as int?) ?? 0;
  }

  static Future<void> removerSincronizadas(List<String> ids) async {
    if (ids.isEmpty) return;
    final database = await db;
    await _garantirColunaFonteFoto(database);
    final placeholders = List.filled(ids.length, '?').join(', ');
    await database.delete(
      _tableCapturas,
      where: 'id IN ($placeholders)',
      whereArgs: ids,
    );
  }

  static Future<void> remover(String id) async {
    final database = await db;
    await _garantirColunaFonteFoto(database);
    await database.delete(_tableCapturas, where: 'id = ?', whereArgs: [id]);
  }

  static Future<void> salvarCacheLista(
    String slug,
    String tipo,
    List<Map<String, dynamic>> dados,
  ) async {
    final database = await db;
    await database.insert(_tableCatalogos, {
      'slug': slug,
      'tipo': tipo,
      'dados': jsonEncode(dados),
      'atualizadoEm': DateTime.now().toIso8601String(),
    }, conflictAlgorithm: ConflictAlgorithm.replace);
  }

  static Future<List<Map<String, dynamic>>?> carregarCacheLista(
    String slug,
    String tipo,
  ) async {
    final database = await db;
    final rows = await database.query(
      _tableCatalogos,
      where: 'slug = ? AND tipo = ?',
      whereArgs: [slug, tipo],
      limit: 1,
    );

    if (rows.isEmpty) return null;

    final dados = rows.first['dados'] as String?;
    if (dados == null || dados.isEmpty) return null;

    final decoded = jsonDecode(dados);
    if (decoded is! List) return null;

    return decoded
        .whereType<Map>()
        .map((e) => Map<String, dynamic>.from(e))
        .toList();
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
      fonteFoto: row['fonteFoto'] as int?,
    );
  }

  static Future<void> _garantirColunaFonteFoto(Database db) async {
    final colunas = await db.rawQuery('PRAGMA table_info($_tableCapturas)');
    final existeFonteFoto = colunas.any(
      (coluna) => (coluna['name'] as String?) == 'fonteFoto',
    );
    if (!existeFonteFoto) {
      await db.execute(
        'ALTER TABLE $_tableCapturas ADD COLUMN fonteFoto INTEGER NULL',
      );
    }
  }
}
