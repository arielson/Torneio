import '../constants.dart';
import 'api_service.dart';
import 'local_db.dart';

class SyncService {
  final ApiService _api;

  SyncService(this._api);

  Future<int> sincronizar(
    String slug,
    String token, {
    bool? sincronizacaoManual,
  }) async {
    final pendentes = await LocalDb.listarPendentes(
      sincronizacaoManual: sincronizacaoManual,
    );
    if (pendentes.isEmpty) return 0;

    final payload = pendentes.map((c) => c.request.toJson()).toList();
    final result = await _api.post(
      ApiConstants.sync(slug),
      payload,
      token: token,
    );
    final sincronizadas = (result?['sincronizadas'] as int?) ?? 0;
    await LocalDb.removerSincronizadas(
      pendentes.take(sincronizadas).map((c) => c.id).toList(),
    );
    return sincronizadas;
  }
}
